﻿using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using SFA.DAS.Commitments.Api.Types.Validation;
using SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetAgreement;
using SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetCommitment;
using SFA.DAS.ProviderApprenticeshipsService.Application.Queries.GetOverlappingApprenticeships;
using SFA.DAS.ProviderApprenticeshipsService.Domain;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Configuration;
using SFA.DAS.ProviderApprenticeshipsService.Web.Orchestrators;
using SFA.DAS.ProviderApprenticeshipsService.Web.Validation;
using SFA.DAS.ProviderApprenticeshipsService.Web.Orchestrators.Mappers;
using CommitmentView = SFA.DAS.Commitments.Api.Types.Commitment.CommitmentView;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.UnitTests.Orchestrators.Commitments
{
    [TestFixture]
    public class WhenFinishEditing : ApprenticeshipValidationTestBase
    {
        private CommitmentView _testCommitment;

        protected override void SetUp()
        {
            _testCommitment = new CommitmentView
            {
                EditStatus = EditStatus.ProviderOnly,
                ProviderId = 1L,
                Apprenticeships = new List<Apprenticeship>
                {
                }
            };
        }

        [Test(Description = "Should return NotReadyForApproval if the commitment is marked as not ready")]
        public void ShouldReturnNotReadyForApprovalWhenCommitmentNotReady()
        {
            _testCommitment.Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship { AgreementStatus = AgreementStatus.ProviderAgreed, CanBeApproved = false },
                    new Apprenticeship { AgreementStatus = AgreementStatus.ProviderAgreed, CanBeApproved = true }
                };

            _mockMediator = GetMediator(_testCommitment);
            _mockMediator.Setup(m => m.SendAsync(It.IsAny<GetProviderAgreementQueryRequest>()))
                .Returns(Task.FromResult(new GetProviderAgreementQueryResponse { HasAgreement = ProviderAgreementStatus.Agreed }));

            var result = _orchestrator.GetFinishEditing(1L, "ABBA123").Result;

            result.ReadyForApproval.Should().BeFalse();
        }

        [Test(Description = "Should return ApproveAndSend if at least one apprenticeship is ProviderAgreed")]
        public void CommitmentWithOneProviderAgreed()
        {
            _testCommitment.CanBeApproved = true;
            _testCommitment.Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship { AgreementStatus = AgreementStatus.BothAgreed },
                    new Apprenticeship { AgreementStatus = AgreementStatus.EmployerAgreed },
                    new Apprenticeship { AgreementStatus = AgreementStatus.ProviderAgreed }
                };

            _mockMediator = GetMediator(_testCommitment);
            var result = _orchestrator.GetFinishEditing(1L, "ABBA123").Result;

            result.IsApproveAndSend.Should().BeTrue();
        }

        [Test(Description = "Should return ApproveAndSend if at least one apprenticeship is NotAgreed")]
        public void CommitmentWithOneNotAgreed()
        {
            _testCommitment.CanBeApproved = true;
            _testCommitment.Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship { AgreementStatus = AgreementStatus.BothAgreed },
                    new Apprenticeship { AgreementStatus = AgreementStatus.EmployerAgreed },
                    new Apprenticeship { AgreementStatus = AgreementStatus.NotAgreed }
                };

            _mockMediator = GetMediator(_testCommitment);

            var result = _orchestrator.GetFinishEditing(1L, "ABBA123").Result;

            result.IsApproveAndSend.Should().BeTrue();
        }

        [Test(Description = "Should return ApproveOnly all are Employer Agreed")]
        public void CommitmentAllEmployerAgreed()
        {
            _testCommitment.CanBeApproved = true;
            _testCommitment.Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship { AgreementStatus = AgreementStatus.EmployerAgreed },
                    new Apprenticeship { AgreementStatus = AgreementStatus.EmployerAgreed },
                    new Apprenticeship { AgreementStatus = AgreementStatus.EmployerAgreed }
                };

            _mockMediator = GetMediator(_testCommitment);
            var result = _orchestrator.GetFinishEditing(1L, "ABBA123").Result;

            result.IsApproveAndSend.Should().BeFalse();
        }

        // --- Helpers ---

        private Mock<IMediator> GetMediator(CommitmentView commitment)
        {
            var respons = new GetCommitmentQueryResponse
            {
                Commitment = commitment
            };

            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.SendAsync(It.IsAny<GetCommitmentQueryRequest>()))
                .Returns(Task.Factory.StartNew(() => respons));

            mockMediator.Setup(m => m.SendAsync(It.IsAny<GetProviderAgreementQueryRequest>()))
                .Returns(Task.FromResult(new GetProviderAgreementQueryResponse { HasAgreement = ProviderAgreementStatus.Agreed }));

            mockMediator.Setup(m => m.SendAsync(It.IsAny<GetOverlappingApprenticeshipsQueryRequest>()))
                .ReturnsAsync(() => new GetOverlappingApprenticeshipsQueryResponse
                {
                    Overlaps = new List<ApprenticeshipOverlapValidationResult>()
                });

            return mockMediator;
        }
    }
}
