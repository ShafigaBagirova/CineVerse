using Application.Tickets.Dtos;
using AutoMapper;
using Domain.Entities;

namespace Application.Common.Mappings;

public sealed class TicketMappingProfile : Profile
{
    public TicketMappingProfile()
    {
        CreateMap<Ticket, GetMyTicketsResponse>()
            .ForMember(dest => dest.MovieId,opt => opt.MapFrom(src => src.Screening.MovieId))
            .ForMember(dest => dest.MovieTitle,opt => opt.MapFrom(src => src.Screening.Movie.Title))
            .ForMember(dest => dest.CinemaId,opt => opt.MapFrom(src => src.Screening.Hall.CinemaId))
            .ForMember(dest => dest.CinemaName,opt => opt.MapFrom(src => src.Screening.Hall.Cinema.Name))
            .ForMember(dest => dest.HallId,opt => opt.MapFrom(src => src.Screening.HallId))
            .ForMember(dest => dest.HallName,opt => opt.MapFrom(src => src.Screening.Hall.Name))
            .ForMember(dest => dest.SeatRow,opt => opt.MapFrom(src => src.Seat.Row))
            .ForMember(dest => dest.SeatNumber,opt => opt.MapFrom(src => src.Seat.Number))
            .ForMember(dest => dest.ScreeningStartTime,opt => opt.MapFrom(src => src.Screening.StartTime))
            .ForMember(dest => dest.ScreeningEndTime,opt => opt.MapFrom(src => src.Screening.EndTime))
            .ForMember(dest => dest.Status,opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<Ticket, GetTicketByIdResponse>()
           .ForMember(dest => dest.MovieId,opt => opt.MapFrom(src => src.Screening.MovieId))
           .ForMember(dest => dest.MovieTitle, opt => opt.MapFrom(src => src.Screening.Movie.Title))
           .ForMember(dest => dest.CinemaId,opt => opt.MapFrom(src => src.Screening.Hall.CinemaId))
           .ForMember(dest => dest.CinemaName, opt => opt.MapFrom(src => src.Screening.Hall.Cinema.Name))
           .ForMember(dest => dest.HallId, opt => opt.MapFrom(src => src.Screening.HallId))
           .ForMember(dest => dest.HallName, opt => opt.MapFrom(src => src.Screening.Hall.Name))
           .ForMember(dest => dest.SeatRow, opt => opt.MapFrom(src => src.Seat.Row))
           .ForMember(dest => dest.SeatNumber,opt => opt.MapFrom(src => src.Seat.Number))
           .ForMember(dest => dest.ScreeningStartTime,opt => opt.MapFrom(src => src.Screening.StartTime))
           .ForMember(dest => dest.ScreeningEndTime,opt => opt.MapFrom(src => src.Screening.EndTime))
           .ForMember(dest => dest.Status,opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<Ticket, GetTicketsByScreeningResponse>()
          .ForMember(dest => dest.MovieId, opt => opt.MapFrom(src => src.Screening.MovieId))
          .ForMember(dest => dest.MovieTitle,opt => opt.MapFrom(src => src.Screening.Movie.Title))
          .ForMember(dest => dest.CinemaId, opt => opt.MapFrom(src => src.Screening.Hall.CinemaId))
          .ForMember(dest => dest.CinemaName,opt => opt.MapFrom(src => src.Screening.Hall.Cinema.Name))
          .ForMember(dest => dest.HallId,opt => opt.MapFrom(src => src.Screening.HallId))
          .ForMember(dest => dest.HallName,opt => opt.MapFrom(src => src.Screening.Hall.Name))
          .ForMember(dest => dest.SeatRow, opt => opt.MapFrom(src => src.Seat.Row))
          .ForMember(dest => dest.SeatNumber, opt => opt.MapFrom(src => src.Seat.Number))
          .ForMember(dest => dest.ScreeningStartTime,opt => opt.MapFrom(src => src.Screening.StartTime))
          .ForMember(dest => dest.ScreeningEndTime, opt => opt.MapFrom(src => src.Screening.EndTime))
          .ForMember(dest => dest.Status,opt => opt.MapFrom(src => src.Status.ToString()));
    }
}