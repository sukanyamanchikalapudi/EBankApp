var dashboardApp = new Vue({
    el: "#accountsContainer",
    data: {
        accounts: []
    },
    created: function () {
        this.fetchData();
    },
    watch: {

    },
    methods: {
        fetchData: function () {
            var url = $("#getAllAccounts").val();
            fetch(url)
                .then(res => { return res.json() })
                .then(res => {
                    let s = JSON.parse(res);
                    console.log(s);
                    s.forEach(u => {
                        this.accounts.push(u);
                    })
                });
        }
    }
});