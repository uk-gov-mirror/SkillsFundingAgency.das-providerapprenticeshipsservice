﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using SFA.DAS.NLog.Logger;
using SFA.DAS.ProviderApprenticeshipsService.Domain;
using SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Services;
using SFA.DAS.ProviderApprenticeshipsService.Web.Models.BulkUpload;
using SFA.DAS.ProviderApprenticeshipsService.Web.Validation;
using SFA.DAS.ProviderApprenticeshipsService.Web.Validation.Text;
using SFA.DAS.Learners.Validators;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Interfaces;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.Orchestrators.BulkUpload
{
    public sealed class InstrumentedBulkUploadValidator : IBulkUploadValidator
    {
        private readonly ILog _logger;
        private readonly IBulkUploadValidator _validator;
        private readonly IAcademicYearValidator _academicYearValidator;

        private readonly ApprenticeshipUploadModelValidator _viewModelValidator;
        
        public InstrumentedBulkUploadValidator(ILog logger, IBulkUploadValidator validator, IUlnValidator ulnValidator, IAcademicYearDateProvider academicYear, IAcademicYearValidator academicYearValidator)
        {
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));
            if (validator == null)
                throw new ArgumentNullException(nameof(validator));

            _logger = logger;
            _validator = validator;
            _academicYearValidator = academicYearValidator;

            _viewModelValidator = new ApprenticeshipUploadModelValidator(new BulkUploadApprenticeshipValidationText(academicYear), new CurrentDateTime(), ulnValidator);
    }

        public IEnumerable<UploadError> ValidateCohortReference(IEnumerable<ApprenticeshipUploadModel> records, string cohortReference)
        {
            var stopwatch = Stopwatch.StartNew();

            var result = _validator.ValidateCohortReference(records, cohortReference);

            _logger.Trace($"Took {stopwatch.ElapsedMilliseconds} milliseconds to validate file for {records.Count()} items");

            return result;
        }

        public IEnumerable<UploadError> ValidateFileSize(HttpPostedFileBase attachment)
        {
            var stopwatch = Stopwatch.StartNew();

            var result = _validator.ValidateFileSize(attachment);

            _logger.Debug($"Took {stopwatch.ElapsedMilliseconds} milliseconds to validate file attributes");

            return result;
        }

        public IEnumerable<UploadError> ValidateRecords(IEnumerable<ApprenticeshipUploadModel> records, List<ITrainingProgramme> trainingProgrammes)
        {
            var stopwatch = Stopwatch.StartNew();

            var result = _validator.ValidateRecords(records, trainingProgrammes);

            _logger.Trace($"Took {stopwatch.ElapsedMilliseconds} milliseconds to validate fields for {records.Count()} items");

            return result;
        }

        public IEnumerable<UploadError> ValidateUlnUniqueness(IEnumerable<ApprenticeshipUploadModel> records)
        {
            var stopwatch = Stopwatch.StartNew();

            var result = _validator.ValidateUlnUniqueness(records);

            _logger.Trace($"Took {stopwatch.ElapsedMilliseconds} milliseconds to validate ULN uniqueness for {records.Count()} items");

            return result;
        }
    }
}