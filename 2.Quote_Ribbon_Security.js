var MyOrg = MyOrg || {};
MyOrg.Quote = MyOrg.Quote || {};

// Quote Ribbon Security
// Scenario:
// Hide "Approve Quote" button if user is NOT Sales Manager

MyOrg.Quote.RibbonSecurity = (function () {

    /**
     * Enable rule function for Ribbon button
     * Returns true -> button visible
     * Returns false -> button hidden
     */
    function isSalesManager() {
        var roles = Xrm.Utility.getGlobalContext().userSettings.roles;
        var hasRole = false;

        roles.forEach(function (role) {
            if (role.name === "Sales Manager") {
                hasRole = true;
            }
        });

        return hasRole;
    }

    return {
        isSalesManager: isSalesManager
    };

})();
