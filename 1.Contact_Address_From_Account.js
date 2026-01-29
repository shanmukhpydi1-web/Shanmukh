var MyOrg = MyOrg || {};
MyOrg.Contact = MyOrg.Contact || {};

// Contact Address Handler
// Scenario:
// Populate Contact address fields when Account lookup changes

MyOrg.Contact.AddressHandler = (function () {

    /** Fired on Account lookup (parentcustomerid) OnChange
     * @param {object} executionContext */
    
	function populateFromAccount(executionContext) {
        var formContext = executionContext.getFormContext();
        var accountLookup = formContext.getAttribute("parentcustomerid").getValue();

        // Exit if Account is cleared or not selected
        if (!accountLookup || accountLookup[0].entityType !== "account") {
            clearContactAddress(formContext);
            return;
        }

        var accountId = accountLookup[0].id.replace("{", "").replace("}", "");

        // Retrieve Account Address
        Xrm.WebApi.retrieveRecord(
            "account",
            accountId,
            "?$select=address1_line1,address1_line2,address1_city,address1_country,address1_postalcode"
        ).then(function (result) {

            setValue(formContext, "address1_line1", result.address1_line1);
            setValue(formContext, "address1_line2", result.address1_line2);
            setValue(formContext, "address1_city", result.address1_city);
            setValue(formContext, "address1_country", result.address1_country);
            setValue(formContext, "address1_postalcode", result.address1_postalcode);

        }).catch(function (error) {
            console.log(error.message);
        });
    }

    // Private Helper Methods (Encapsulation)

    function setValue(formContext, fieldName, value) {
        var attribute = formContext.getAttribute(fieldName);
        if (attribute) {
            attribute.setValue(value || null);
        }
    }

    function clearContactAddress(formContext) {
        setValue(formContext, "address1_line1", null);
        setValue(formContext, "address1_line2", null);
        setValue(formContext, "address1_city", null);
        setValue(formContext, "address1_country", null);
        setValue(formContext, "address1_postalcode", null);
    }

    return {
        populateFromAccount: populateFromAccount
    };

})();

