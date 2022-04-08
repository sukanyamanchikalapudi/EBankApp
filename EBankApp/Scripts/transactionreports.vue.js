var dashboardApp = new Vue({
    el: "#transactionReportApp",
    data: {
        currencies: [] = [],
        accounts: [] = [],
        accountNumbers: [] = [],
        relativeReports: [{
            Value: 0,
            Text: "This week"
        }, {
            Value: 1,
            Text: "This Month"
        }],
        relativeReport: {
            accountNumber: "",
            reportType: 0
        },
        customReport: {
            accountNumber: "",
            start: "",
            end: ""
        }
    },
    created: function () {
        this.fetchAccounts();
    },
    watch: {

    },
    methods: {
        fetchAccounts: function () {
            var id = $("#userId").val();
            var url = $("#accountsUrl").val();
            fetch(url + "?userId=" + id)
                .then(res => { return res.json() })
                .then(res => {
                    res.Accounts.forEach(a => {
                        this.accounts.push({
                            accountNumber: a.AccountNumber,
                            accountBalance: a.AccountBalance,
                            accountType: a.AccountType
                        });
                    })
                    res.AccountNumbers.forEach(n => {
                        this.accountNumbers.push(n);
                    });
                    this.user = res.user;
                });
        },
        downloadCustomReport: function () {
            let self = this;
            console.log(this.customReport);
            let url = $("#printTransactionUrl").val();
            let userId = $("#userId").val();


            let data = {
                AccountNumber: self.customReport.accountNumber,
                Start: $("#startDate").val(),
                End: $("#endDate").val(),
                UserId: userId
            };

            $.post(url, data).done(function (resp) {
                window.location.href = resp.RedirectUrl;
            }).fail(function (resp) {
                window.location.href = resp.RedirectUrl;
            }).always(function () {
                self.fetchAccounts();
            });
        },
        downloadRelativeReport: function () {
            let self = this;
            console.log(this.relativeReport);
            let url = $("#printTransactionUrl").val();
            let userId = $("#userId").val();

            let data = {
                AccountNumber: self.relativeReport.accountNumber,
                ReportType: self.relativeReport.reportType,
                UserId: userId
            };

            $.post(url, data).done(function (resp) {
                window.location.href = resp.RedirectUrl;
            }).fail(function (resp) {
                window.location.href = resp.RedirectUrl;
            }).always(function () {
            });
        }
    }
});