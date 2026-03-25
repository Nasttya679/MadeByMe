using MadeByMe.Application.Common;
using MadeByMe.Application.DTOs;
using MadeByMe.Domain.Entities;

namespace MadeByMe.Application.Services.Interfaces
{
    public interface IApplicationUserService
    {
        Result<ApplicationUser> UpdateUser(string userId, UpdateProfileDto dto);
    }
}