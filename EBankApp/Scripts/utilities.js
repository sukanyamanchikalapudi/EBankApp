var utilitiesApp = (function () {

    function formatStringAsCurrency(amount, currency) {
        switch (currency) {
            case 2:
                return amount.toLocaleString("en-US", { style: 'currency', currency: "USD" });
                break;
            default:
                return amount.toLocaleString("en-GB", { style: 'currency', currency: "GBP" });
        }
    }

    function reloadDataTableWithoutPagingReset(table) {
        return table.ajax.reload(null, false);
    }

    function reloadDataTableWithPagingReset(table) {
        return table.ajax.reload(null, true);
    }

    function getAccountTypeText(accountType) {
        switch (parseInt(accountType)) {
            case 1:
                return "CURRENT"
            case 2:
                return "SAVINGS"
            default:
                return "-"
        }
    }

    return {
        formatCurrency: formatStringAsCurrency,
        refreshTableWithoutPagingReset: reloadDataTableWithoutPagingReset,
        refreshTableWithPagingReset: reloadDataTableWithPagingReset,
        getAccountType: getAccountTypeText
    }
})();