using ShareStudy.Domain.Entities;

namespace ShareStudy.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, int ExpiresInSeconds) GenerateAccessToken(User user, IEnumerable<string> roles);
    RefreshToken GenerateRefreshToken(long userId);
}
