var MyOrg = MyOrg || {};
MyOrg.Opportunity = MyOrg.Opportunity || {};

// Opportunity Estimated Revenue Security
// Scenario:
// Lock Estimated Revenue for all users except Finance

MyOrg.Opportunity.EstimatedRevenueSecurity = (function () {

    /**
     * Fired on Opportunity Form OnLoad
     * @param {object} executionContext
     */
    function lockEstimatedRevenue(executionContext) {
        var formContext = executionContext.getFormContext();
        var roles = Xrm.Utility.getGlobalContext().userSettings.roles;
        var isFinanceUser = false;

        // Check user roles
        roles.forEach(function (role) {
            if (role.name === "Finance") {
                isFinanceUser = true;
            }
        });

        // Lock field for non-finance users
        if (!isFinanceUser) {
            var control = formContext.getControl("estimatedvalue");
            if (control) {
                control.setDisabled(true);
            }
        }
    }

    return {
        lockEstimatedRevenue: lockEstimatedRevenue
    };

})();


