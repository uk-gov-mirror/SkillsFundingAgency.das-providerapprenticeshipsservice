﻿using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using SFA.DAS.ProviderApprenticeshipsService.Application.Commands.CreateApprenticeship;
using SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetCommitment;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Configuration;
using SFA.DAS.ProviderApprenticeshipsService.Web.Models;
using SFA.DAS.ProviderApprenticeshipsService.Web.Orchestrators;
using SFA.DAS.ProviderApprenticeshipsService.Web.Validation;
using SFA.DAS.ProviderApprenticeshipsService.Web.Orchestrators.Mappers;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.UnitTests.Orchestrators.Commitments
{
    [TestFixture]
    public sealed class WhenCreatingApprenticeship : ApprenticeshipValidationTestBase
    {

        [Test]
        public async Task ShouldCallMediatorToCreate()
        {
            var expectedCommitmentId = 1234;
            var expectedApprenticeshipId = 9876;
            var viewModel = new Web.Models.ApprenticeshipViewModel
            {
                ProviderId = 123L,
                HashedApprenticeshipId = "ABC123",
                HashedCommitmentId = "ZZZ999"
            };
            var signedInUser = new SignInUserModel() { DisplayName = "Bob", Email = "bob@test.com" };

            _mockHashingService.Setup(x => x.DecodeValue(viewModel.HashedApprenticeshipId)).Returns(expectedApprenticeshipId);
            _mockHashingService.Setup(x => x.DecodeValue(viewModel.HashedCommitmentId)).Returns(expectedCommitmentId);

            _mockMediator.Setup(x => x.Send(It.Is<GetCommitmentQueryRequest>(y => y.ProviderId == viewModel.ProviderId && y.CommitmentId == expectedCommitmentId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCommitmentQueryResponse { Commitment = new CommitmentView { EditStatus = EditStatus.ProviderOnly, AgreementStatus = AgreementStatus.EmployerAgreed } });

            _mockMapper.Setup(m => m.MapApprenticeship(It.IsAny<ApprenticeshipViewModel>()))
                .ReturnsAsync(new Apprenticeship { ProviderId = 123L, Id = expectedApprenticeshipId, CommitmentId = expectedCommitmentId});

            await _orchestrator.CreateApprenticeship("user123", viewModel, signedInUser);

            _mockMediator.Verify(
                x =>
                    x.Send(
                        It.Is<CreateApprenticeshipCommand>(
                            c =>
                                c.ProviderId == viewModel.ProviderId && c.UserId == "user123" && c.Apprenticeship != null && c.UserDisplayName == signedInUser.DisplayName &&
                                c.UserEmailAddress == signedInUser.Email), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
