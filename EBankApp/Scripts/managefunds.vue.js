var manageFundsApp = new Vue({
    el: "#manageFundsApp",
    data: {
        currencies: [] = [],
        accounts: [] = [],
        accountNumbers: [] = [],
        user: {},
        exchangeMoney: {
            accountNumber: "",
            amount: "",
            from: "",
            to: "",
            userId: ""
        },
        transferMoney: {
            accountNumber: "",
            amount: "",
            currency: "",
            from: "",
            to: "",
            userId: ""
        },
        modelStateErrors: [] = []
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
                    this.currencies = [];
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
                    this.accounts = [];
                    this.accountNumbers = [];
                    res.Accounts.forEach(a => {
                        this.accounts.push({
                            accountNumber: a.AccountNumber,
                            accountBalance: utilitiesApp.formatCurrency(a.AccountBalance, a.Currency),
                            accountType: a.AccountType
                        });
                    })
                    res.AccountNumbers.forEach(n => {
                        this.accountNumbers.push(n);
                    });
                    this.user = res.user;
                });
        },
        exchangeMoneyFunc: function () {
            let self = this;
            console.log(this.transferMoney);
            let url = $("#exchangeCurrencyUrl").val();
            let userId = $("#userId").val();
            let data = {
                AccountNumber: self.exchangeMoney.accountNumber,
                From: self.exchangeMoney.from,
                To: self.exchangeMoney.to,
                Amount: self.exchangeMoney.amount,
                UserId: userId
            };

            $.post(url, data).done(function (resp) {
                if (resp.RedirectUrl == "" && resp.ErrorMessages != null) {
                    self.modelStateErrors = [...resp.ErrorMessages];
                    console.log(self.modelStateErrors);
                } else {
                    window.location.href = resp.RedirectUrl;
                }
            }).fail(function (resp) {
                window.location.href = resp.RedirectUrl;
            }).always(function () {
                self.fetchAccounts();
            });
        },
        interBankTransferMoney: function () {
            let self = this;
            console.log(this.exchangeMoney);
            let url = $("#interBankTransferMoneyUrl").val();
            let userId = $("#userId").val();

            let data = {
                AccountNumber: self.transferMoney.accountNumber,
                Source: self.transferMoney.from,
                Destination: self.transferMoney.to,
                Amount: self.transferMoney.amount,
                UserId: userId,
                Currency: self.transferMoney.currency
            };

            $.post(url, data).done(function (resp) {
                if (resp.RedirectUrl == "" && resp.ErrorMessages != null) {
                    this.modelStateErrors = [];
                    resp.ErrorMessages.forEach(e => {
                        this.modelStateErrors.push(e);
                    });
                } else {
                    window.location.href = resp.RedirectUrl;
                }
            }).fail(function (resp) {
                window.location.href = resp.RedirectUrl;
            }).always(function () {
                self.fetchAccounts();
            });
        }
    }
});