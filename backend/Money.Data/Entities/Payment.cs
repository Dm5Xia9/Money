using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Money.Data.Entities;

[PrimaryKey(nameof(UserId), nameof(Id))]
public class Payment : IUserEntity
{
    [Key]
    [Column(Order = 1)]
    public int UserId { get; set; }

    [Key]
    [Column(Order = 2)]
    public int Id { get; set; }

    public decimal Sum { get; set; }

    public int CategoryId { get; set; }

    [StringLength(4000)]
    public string Comment { get; set; }

    public DateTime Date { get; set; }

    /// <summary>
    ///    ������������� ���������� ������.
    /// </summary>
    /// <remarks>
    ///    � ����� ������� �������� ��� ��������: �������(TaskId=null) � ���������� ������(TaskId!=null)
    /// </remarks>
    public int? TaskId { get; set; }

    /// <summary>
    ///    ������������� ������������ ���������� ������.
    /// </summary>
    /// <remarks>
    ///    �� null, ���� ����� ������ ���������� �������.
    /// </remarks>
    public int? CreatedTaskId { get; set; }

    public int? PlaceId { get; set; }

    public DomainUser? User { get; set; }

    public bool IsDeleted { get; set; }
}
