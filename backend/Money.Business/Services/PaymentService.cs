﻿using Microsoft.EntityFrameworkCore;
using Money.Business.Models;
using Money.Common.Exceptions;
using Money.Data;
using Money.Data.Extensions;

namespace Money.Business.Services;

public class PaymentService(RequestEnvironment environment, ApplicationDbContext context, CategoryService categoryService)
{
    public async Task<ICollection<Payment>> GetAsync(PaymentFilter filter, CancellationToken cancellationToken = default)
    {
        IQueryable<Data.Entities.Payment> dbPayments = FilterPayments(filter);

        List<int> placeIds = await dbPayments
            .Where(x => x.PlaceId != null)
            .Select(x => x.PlaceId!.Value)
            .ToListAsync(cancellationToken);

        List<Data.Entities.Place> dbPlaces = await GetPlacesAsync(placeIds, cancellationToken);

        List<Data.Entities.Payment> dbPaymentList = await dbPayments
            .OrderByDescending(x => x.Date)
            .ThenBy(x => x.CategoryId)
            .ToListAsync(cancellationToken);

        return dbPaymentList.Select(dbPayment => MapTo(dbPayment, dbPlaces)).ToList();
    }

    public async Task<Payment> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        Data.Entities.Payment dbPayment = await GetByIdInternal(id, cancellationToken);

        List<Data.Entities.Place> dbPlaces = dbPayment.PlaceId != null
            ? await GetPlacesAsync([dbPayment.PlaceId.Value], cancellationToken)
            : [];

        Payment payment = MapTo(dbPayment, dbPlaces);
        return payment;
    }

    private async Task<Data.Entities.Payment> GetByIdInternal(int id, CancellationToken cancellationToken)
    {
        Data.Entities.Payment dbCategory = await context.Payments.SingleOrDefaultAsync(environment.UserId, id, cancellationToken)
                                           ?? throw new NotFoundException("платеж", id);

        return dbCategory;
    }

    private IQueryable<Data.Entities.Payment> FilterPayments(PaymentFilter filter)
    {
        IQueryable<Data.Entities.Payment> dbPayments = context.Payments.IsUserEntity(environment.UserId)
            .Where(x => x.TaskId == null);

        if (filter.DateFrom.HasValue)
        {
            dbPayments = dbPayments.Where(x => x.Date >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            dbPayments = dbPayments.Where(x => x.Date < filter.DateTo.Value);
        }

        if (filter.CategoryIds is { Count: > 0 })
        {
            dbPayments = dbPayments.Where(x => filter.CategoryIds.Contains(x.CategoryId));
        }

        if (string.IsNullOrEmpty(filter.Comment) == false)
        {
            dbPayments = dbPayments.Where(x => x.Comment != null && x.Comment.Contains(filter.Comment));
        }

        if (string.IsNullOrEmpty(filter.Place) == false)
        {
            IQueryable<int> placesIds = context.Places
                .Where(x => x.UserId == environment.UserId && x.Name.Contains(filter.Place))
                .Select(x => x.Id);

            dbPayments = dbPayments.Where(x => x.PlaceId != null && placesIds.Contains(x.PlaceId.Value));
        }

        return dbPayments;
    }

    private async Task<List<Data.Entities.Place>> GetPlacesAsync(List<int> placeIds, CancellationToken cancellationToken)
    {
        return await context.Places
            .Where(x => x.UserId == environment.UserId && placeIds.Contains(x.Id))
            .ToListAsync(cancellationToken);
    }

    private Payment MapTo(Data.Entities.Payment dbPayment, List<Data.Entities.Place> dbPlaces)
    {
        return new Payment
        {
            CategoryId = dbPayment.CategoryId,
            Sum = dbPayment.Sum,
            Comment = dbPayment.Comment,
            Place = dbPayment.PlaceId.HasValue
                ? dbPlaces.FirstOrDefault(x => x.Id == dbPayment.PlaceId)?.Name
                : null,
            Id = dbPayment.Id,
            Date = dbPayment.Date,
            CreatedTaskId = dbPayment.CreatedTaskId,
        };
    }

    public async Task<int> CreateAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        if (environment.UserId == null)
        {
            throw new BusinessException("Извините, но идентификатор пользователя не указан.");
        }

        Data.Entities.DomainUser dbUser = await context.DomainUsers.SingleAsync(x => x.Id == environment.UserId, cancellationToken);

        int paymentId = dbUser.NextPaymentId;
        dbUser.NextPaymentId++;

        int? placeId = await GetPlaceId(dbUser, payment.Place, cancellationToken);

        Data.Entities.Payment dbPayment = new()
        {
            Id = paymentId,
            UserId = environment.UserId.Value,
            CategoryId = payment.CategoryId,
            Sum = payment.Sum,
            Comment = payment.Comment,
            Date = payment.Date,
            PlaceId = placeId,
            CreatedTaskId = payment.CreatedTaskId,
        };

        await context.Payments.AddAsync(dbPayment, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
        return paymentId;
    }

    public async Task<int?> GetPlaceId(Data.Entities.DomainUser dbUser, string? place, CancellationToken cancellationToken = default)
    {
        place = place?.Trim(' ');

        if (string.IsNullOrEmpty(place))
        {
            return null;
        }

        Data.Entities.Place? dbPlace = await context.Places
            .IsUserEntity(environment.UserId)
            .FirstOrDefaultAsync(x => x.Name == place, cancellationToken);

        if (dbPlace == null)
        {
            int newPlaceId = dbUser.NextPlaceId;
            dbUser.NextPlaceId++;

            dbPlace = new Data.Entities.Place
            {
                UserId = dbUser.Id,
                Id = newPlaceId,
                Name = place,
            };

            await context.Places.AddAsync(dbPlace, cancellationToken);
        }

        dbPlace.LastUsedDate = DateTime.Now;
        dbPlace.Name = place;
        return dbPlace.Id;
    }

    public async Task UpdateAsync(Payment payment, CancellationToken cancellationToken)
    {
        Data.Entities.Payment dbPayment = await context.Payments.SingleOrDefaultAsync(environment.UserId, payment.Id, cancellationToken)
                                          ?? throw new NotFoundException("платеж", payment.Id);

        Category category = await categoryService.GetByIdAsync(payment.CategoryId, cancellationToken);
        Data.Entities.DomainUser dbUser = await context.DomainUsers.SingleAsync(x => x.Id == environment.UserId, cancellationToken);
        int? placeId = await GetPlaceId(dbUser, payment.Place, dbPayment, cancellationToken);

        dbPayment.Sum = payment.Sum;
        dbPayment.Comment = payment.Comment;
        dbPayment.Date = payment.Date;
        dbPayment.CategoryId = category.Id;
        dbPayment.PlaceId = placeId;

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int?> GetPlaceId(Data.Entities.DomainUser dbUser, string? place, Data.Entities.Payment dbPayment, CancellationToken cancellationToken = default)
    {
        Data.Entities.Place? dbPlace = dbPayment.PlaceId != null
            ? context.Places.Single(x => x.UserId == dbUser.Id && x.Id == dbPayment.PlaceId)
            : null;

        bool hasAnyPayments = false;

        if (dbPlace != null)
        {
            hasAnyPayments = await IsPlaceBusy(dbPlace.Id, dbUser.Id, dbPayment.Id, cancellationToken);
        }

        int? placeId;

        if (!string.IsNullOrEmpty(place))
        {
            Data.Entities.Place? dbNewPlace = await context.Places
                .IsUserEntity(dbUser.Id)
                .SingleOrDefaultAsync(x =>  x.Name == place, cancellationToken);

            if (dbNewPlace == null)
            {
                if (dbPlace != null && !hasAnyPayments)
                {
                    dbNewPlace = dbPlace;
                }
                else
                {
                    int newPlaceId = dbUser.NextPlaceId;
                    dbUser.NextPlaceId++;

                    dbNewPlace = new Data.Entities.Place
                    {
                        Name = "",
                        UserId = dbUser.Id,
                        Id = newPlaceId,
                    };

                    await context.Places.AddAsync(dbNewPlace, cancellationToken);
                }

                dbNewPlace.LastUsedDate = DateTime.Now;
            }
            else
            {
                dbNewPlace.LastUsedDate = DateTime.Now;

                if (dbPlace != null && !hasAnyPayments && dbPlace.Id != dbNewPlace.Id)
                {
                    dbPlace.IsDeleted = true;
                }
            }

            dbNewPlace.IsDeleted = false;
            dbNewPlace.Name = place;
            placeId = dbNewPlace.Id;
        }
        else
        {
            placeId = null;

            if (dbPlace != null && !hasAnyPayments)
            {
                dbPlace.IsDeleted = true;
            }
        }

        return placeId;
    }

    public Task<bool> IsPlaceBusy(int placeId, int userId, int? paymentId, CancellationToken cancellationToken = default)
    {
        IQueryable<Data.Entities.Payment> payments = context.Payments.Where(x => x.UserId == userId && x.PlaceId == placeId);

        if (paymentId.HasValue)
        {
            payments = payments.Where(x => x.Id != paymentId.Value);
        }

        return payments.AnyAsync(cancellationToken);
    }


    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        Data.Entities.Payment dbPayment = await GetByIdInternal(id, cancellationToken);
        dbPayment.IsDeleted = true;
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        Data.Entities.Payment dbPayment = await context.Payments
                                              .IgnoreQueryFilters()
                                              .Where(x => x.IsDeleted)
                                              .SingleOrDefaultAsync(environment.UserId, id, cancellationToken)
                                          ?? throw new NotFoundException("платеж", id);

        dbPayment.IsDeleted = false;
        await context.SaveChangesAsync(cancellationToken);
    }
}
