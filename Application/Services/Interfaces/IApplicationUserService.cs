using MadeByMe.Application.DTOs;
using MadeByMe.Domain.Entities;


namespace MadeByMe.Application.Services.Interfaces
{
    public interface IApplicationUserService
    {
        ApplicationUser UpdateUser(string userId, UpdateProfileDto dto);
    }
}
