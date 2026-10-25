using System;
using System.Globalization;
using Smoke.Mapping.Dtos;
using Transformations.Mapping;

namespace Smoke.Mapping.Domain;

[MapTo<TicketDto>]
public partial class Ticket
{
    public int Id { get; set; }

    [MapProperty("OpenedOn", Converter = nameof(FormatOpened))]
    public DateTime Opened { get; set; }

    private static string FormatOpened(DateTime value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
