﻿namespace SFA.DAS.ProviderApprenticeshipsService.Web.Tests.Orchestrators
{
    using System.Threading.Tasks;

    using MediatR;

    using Moq;

    using NUnit.Framework;

    using Application.Commands.SubmitCommitment;
    using Models.Types;

    using SFA.DAS.Commitments.Api.Types;
    using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces;

    using Web.Orchestrators;

    [TestFixture]
    public class WhenApproveCommitment
    {
        [TestCase(SaveStatus.Save, AgreementStatus.NotAgreed, true)]
        [TestCase(SaveStatus.ApproveAndSend, AgreementStatus.ProviderAgreed, true)]
        [TestCase(SaveStatus.AmendAndSend, AgreementStatus.ProviderAgreed, true)]
        [TestCase(SaveStatus.Approve, AgreementStatus.ProviderAgreed, false)]
        public async Task  CheckStatusUpdate(SaveStatus input, AgreementStatus expectedAgreementStatus, bool expectedCreateTaskBool)
        {
            var mockMediator = new Mock<IMediator>();
            var mockHashingService = new Mock<IHashingService>();
            mockHashingService.Setup(m => m.DecodeValue("ABBA99")).Returns(2L);

            var _sut = new CommitmentOrchestrator(mockMediator.Object, Mock.Of<ICommitmentStatusCalculator>(), mockHashingService.Object);
            await _sut.ApproveCommitment(1L, "ABBA99", input);

            mockMediator.Verify(m => m
                .SendAsync(It.Is<SubmitCommitmentCommand>(
                    p => p.ProviderId == 1L &&
                    p.CommitmentId == 2L &&
                    p.Message == string.Empty &&
                    p.AgreementStatus == expectedAgreementStatus &&
                    p.CreateTask == expectedCreateTaskBool )));
        }
    }
}
