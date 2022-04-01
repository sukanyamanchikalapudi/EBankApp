var dashboardApp = new Vue({
    el: "#manageFundsApp",
    data: {
        currencies: [] = [],
        accounts: [] = [],
        user: {},
        exchangeMoney: {
            accountNumber: "",
            amount: "",
            From: "",
            To: ""
        }
    },
    created: function () {
        this.fetchCurrencies();
        this.fetchAccounts();
    },
    watch: {

    },
    methods: {
        fetchCurrencies: function () {
            var url = $("#currenciesUrl").val();
            fetch(url)
                .then(res => { return res.json() })
                .then(res => {
                    let s = JSON.parse(res);
                    s.forEach(c => {
                        this.currencies.push(c);
                    })
                });
        },
        fetchAccounts: function () {
            var id = $("#userId").val();
            var url = $("#accountsUrl").val();
            fetch(url + "?userId=" + id)
                .then(res => { return res.json() })
                .then(res => {
                    console.log(res);
                    res.Accounts.forEach(a => {
                        console.log(a);
                        this.accounts.push({
                            accountNumber: a.AccountNumber,
                            accountBalance: a.AccountBalance,
                            accountType: a.AccountType
                        });
                    })
                    this.user = res.user;
                });
        },
        exchangeMoneyFunc: function () {
            console.log(this.exchangeMoney);
        }
    }
});