using Application.Common.Responses;
using Application.Halls.Commands;
using Microsoft.AspNetCore.Mvc;

namespace Application.Halls.Dtos;

public class GetHallByIdResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int CinemaId { get; set; }

}