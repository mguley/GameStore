using System.ComponentModel.DataAnnotations;

namespace GameStore.Api.Dtos;

public record CreateGameDto(
    [Required][StringLength(25)] string Name,
    int GenreId,
    [Range(1, 100)] decimal Price,
    DateOnly ReleaseDate);