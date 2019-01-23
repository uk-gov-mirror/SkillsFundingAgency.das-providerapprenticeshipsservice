﻿using System.Collections.Generic;
using SFA.DAS.HashingService;
using SFA.DAS.ProviderApprenticeshipsService.Web.Models;
using SFA.DAS.ProviderRelationships.Types.Dtos;

namespace SFA.DAS.ProviderApprenticeshipsService.Web.Orchestrators.Mappers
{
    public class CreateCohortMapper : ICreateCohortMapper
    {
        private readonly IHashingService _hashingService;

        public CreateCohortMapper(IHashingService hashingService)
        {
            _hashingService = hashingService;
        }

        public ChooseEmployerViewModel Map(IEnumerable<AccountProviderLegalEntityDto> source)
        {
            var result = new ChooseEmployerViewModel();

            var legalEntities = new List<LegalEntityViewModel>();

            foreach (var relationship in source)
            {
                legalEntities.Add(new LegalEntityViewModel
                {
                    EmployerAccountPublicHashedId = relationship.AccountPublicHashedId,
                    EmployerAccountName = relationship.AccountName,
                    EmployerAccountLegalEntityPublicHashedId = relationship.AccountLegalEntityPublicHashedId,
                    EmployerAccountLegalEntityName = relationship.AccountLegalEntityName  
                });
            }

            result.LegalEntities = legalEntities;

            return result;
        }
    }
}