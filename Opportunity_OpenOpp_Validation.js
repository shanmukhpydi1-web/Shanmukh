var MyOrg = MyOrg || {};
MyOrg.Opportunity = MyOrg.Opportunity || {};

// Opportunity Open Opportunity Validation
// Scenario:
// If Account has more than 5 open Opportunities then Lock Probability and Show warning message

MyOrg.Opportunity.OpenOppValidation = (function () {

    /**
     * Fired on Opportunity Form OnLoad
     * @param {object} executionContext
     */
    function validateOpenOpportunities(executionContext) {
        var formContext = executionContext.getFormContext();
        var accountLookup = formContext.getAttribute("parentaccountid").getValue();

        // Exit if no Account selected
        if (!accountLookup) {
            return;
        }

        var accountId = accountLookup[0].id.replace("{", "").replace("}", "");

        // Query to get open opportunities for the account
        var query =
            "?$select=opportunityid" +
            "&$filter=_parentaccountid_value eq " + accountId +
            " and statecode eq 0"; // 0 = Open

        Xrm.WebApi.retrieveMultipleRecords("opportunity", query)
            .then(function (result) {
                if (result.entities.length > 5) {
                    lockProbability(formContext);
                    showWarning(formContext);
                }
            })
            .catch(function (error) {
                console.log(error.message);
            });
    }

    // Private Helper Methods
   
    function lockProbability(formContext) {
        var control = formContext.getControl("closeprobability");
        if (control) {
            control.setDisabled(true);
        }
    }

    function showWarning(formContext) {
        formContext.ui.setFormNotification(
            "This account already has more than 5 open opportunities. Probability is locked.",
            "WARNING",
            "OpenOpportunityLimit"
        );
    }

    return {
        validateOpenOpportunities: validateOpenOpportunities
    };

})();

