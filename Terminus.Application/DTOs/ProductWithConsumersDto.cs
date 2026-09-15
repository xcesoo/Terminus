namespace Terminus.Application.DTOs;

public record ProductWithConsumersDto(string ProductName, IEnumerable<string> ConsumerNames);