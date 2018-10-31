﻿using MediatR;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Commands.CreateApprenticeship
{
    public class CreateApprenticeshipCommand : IRequest
    {
        public string UserId { get; set; }
        public long ProviderId { get; set; }
        public Apprenticeship Apprenticeship { get; set; }
        public string UserEmailAddress { get; set; }
        public string UserDisplayName { get; set; }
    }
}