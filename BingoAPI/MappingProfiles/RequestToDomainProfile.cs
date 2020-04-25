using AutoMapper;
using Bingo.Contracts.V1.Requests.User;
using BingoAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BingoAPI.MappingProfiles
{
    public class RequestToDomainProfile : Profile
    {
        public RequestToDomainProfile()
        {
            CreateMap<UpdateUserRequest, AppUser>();
        }
    }
}
