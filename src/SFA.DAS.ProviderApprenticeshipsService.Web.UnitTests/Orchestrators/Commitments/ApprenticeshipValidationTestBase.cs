using NUnit.Framework;
using SFA.DAS.Learners.Validators;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Services;
using SFA.DAS.ProviderApprenticeshipsService.Web.Models;
using SFA.DAS.ProviderApprenticeshipsService.Web.Validation;
using SFA.DAS.ProviderApprenticeshipsService.Web.Validation.Text;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.UnitTests.Orchestrators.Commitments
{
    public abstract class ApprenticeshipValidationTestBase
    {
        protected readonly ApprenticeshipViewModelValidator Validator = new ApprenticeshipViewModelValidator(
                                                                        new WebApprenticeshipValidationText(new Infrastructure.Services.AcademicYear(new CurrentDateTime())), 
                                                                        new CurrentDateTime(), 
                                                                        new Infrastructure.Services.AcademicYear(new CurrentDateTime()),
                                                                        new UlnValidator(),
                                                                        new AcademicYearValidator(new CurrentDateTime(), new Infrastructure.Services.AcademicYear(new CurrentDateTime())));

        protected ApprenticeshipViewModel ValidModel;

        [SetUp]
        public void BaseSetup()
        {
            ValidModel = new ApprenticeshipViewModel { ULN = "1001234567", FirstName = "TestFirstName", LastName = "TestLastName" };
        }
    }
}