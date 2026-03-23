using Application.Common.Responses;
using Application.Screenings.Queries;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Application.Screenings.Dtos;

public class GetAllScreeningsRequest
{
    public int? MovieId { get; set; }
    public int? HallId { get; set; }
    public ScreeningStatus? Status { get; set; }
    public ScreeningFormat? Format { get; set; }
    public bool? IsActive { get; set; }

    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}