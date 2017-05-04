﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.ProviderApprenticeshipsService.Application.Commands.BulkUploadApprenticeships;

namespace SFA.DAS.ProviderApprenticeshipsService.Application.UnitTests.Commands.BulkUploadApprenticeships
{
    [TestFixture]
    public sealed class WhenBulkUploading
    {
        private Mock<IProviderCommitmentsApi> _mockCommitmentsApi;
        private BulkUploadApprenticeshipsCommand _exampleValidCommand;
        private BulkUploadApprenticeshipsCommandHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockCommitmentsApi = new Mock<IProviderCommitmentsApi>();
            _exampleValidCommand = new BulkUploadApprenticeshipsCommand
            {
                UserId = "user123",
                ProviderId = 123L,
                CommitmentId = 333L,
                Apprenticeships = new List<Apprenticeship>
                {
                    new Apprenticeship { FirstName = "John", LastName = "Smith" },
                    new Apprenticeship { FirstName = "Emma", LastName = "Jones" },
                },
                UserEmailAddress = "test@email.com",
                UserDisplayName = "Bob"
            };

            _handler = new BulkUploadApprenticeshipsCommandHandler(_mockCommitmentsApi.Object);
        }

        [Test]
        public async Task ShouldCallCommitmentApi()
        {
            await _handler.Handle(_exampleValidCommand);

            _mockCommitmentsApi.Verify(
                x =>
                    x.BulkUploadApprenticeships(_exampleValidCommand.ProviderId, _exampleValidCommand.CommitmentId,
                        It.Is<BulkApprenticeshipRequest>(
                            r =>
                                r.Apprenticeships == _exampleValidCommand.Apprenticeships && r.UserId == _exampleValidCommand.UserId &&
                                r.LastUpdatedByInfo.EmailAddress == _exampleValidCommand.UserEmailAddress && r.LastUpdatedByInfo.Name == _exampleValidCommand.UserDisplayName)));
        }

        [Test]
        public void ShouldThrowAnExceptionOnValidationFailure()
        {
            _exampleValidCommand.ProviderId = 0; // This is invalid

            Func<Task> act = async () => { await _handler.Handle(_exampleValidCommand); };

            act.ShouldThrow<InvalidRequestException>();
        }
    }
}
