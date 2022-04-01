var dashboardApp = new Vue({
    el: "#usersContainer",
    data: {
        users: []
    },
    created: function () {
        this.fetchData();
    },
    watch: {

    },
    methods: {
        fetchData: function () {
            var url = $("#getAllUsers").val();
            fetch(url)
                .then(res => { return res.json() })
                .then(res => {
                    let s = JSON.parse(res);
                    s.forEach(u => {
                        this.users.push(u);
                    })
                });
        }
    }
});