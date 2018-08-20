﻿
namespace SFA.DAS.ProviderApprenticeshipsService.Web.Models.Agreement
{
    public class CommitmentAgreement
    {
        public string OrganisationName { get; set; }    // LegalEntityName
        public string CohortID { get; set; }            // HashedCommitmentId
        public string AgreementID { get; set; }         // AccountLegalEntityPublicHashedId
    }
}