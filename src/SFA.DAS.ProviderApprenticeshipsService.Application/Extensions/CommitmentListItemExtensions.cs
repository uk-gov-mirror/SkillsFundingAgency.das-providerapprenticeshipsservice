﻿using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.ProviderApprenticeshipsService.Application.Domain.Commitment;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.Extensions
{
    /// <remarks>
    /// Ideally would belong on CommitmentListItem itself, but this is the next best thing!
    /// </remarks>
    public static class CommitmentListItemExtensions
    {
        private static readonly CommitmentStatusCalculator StatusCalculator = new CommitmentStatusCalculator();

        public static RequestStatus GetStatus(this CommitmentListItem commitment)
        {
            return StatusCalculator.GetStatus(
                commitment.EditStatus,
                commitment.ApprenticeshipCount,
                commitment.LastAction,
                commitment.AgreementStatus,
                commitment.ProviderLastUpdateInfo);
            //commitment.TransferSenderId,
            //commitment.TransferApprovalStatus);
        }
    }
}
