﻿namespace Money.Web.Models;

public static class PaymentTypes
{
    public static Value[] Values { get; } = GetValues();

    // представим, что мы их получили с бэкэнда
    private static Value[] GetValues()
    {
        return
        [
            new Value
            {
                Id = 1,
                Name = "Расходы",
                Icon = Icons.Material.Rounded.ArrowCircleDown,
                Color = Color.Warning,
            },
            new Value
            {
                Id = 2,
                Name = "Доходы",
                Icon = Icons.Material.Rounded.ArrowCircleUp,
                Color = Color.Success,
            },
        ];
    }

    public class Value
    {
        public required int Id { get; init; }
        public required string Name { get; init; }
        public required string Icon { get; set; }
        public required Color Color { get; set; }
    }
}
