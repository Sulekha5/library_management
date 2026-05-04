document.addEventListener("DOMContentLoaded", function () {

    console.log("Manager JS Loaded");

    // DASHBOARD DATA
    fetch('/Home/GetDashboardData')
        .then(res => res.json())
        .then(data => {
            document.getElementById("totalBooks").innerText = data.totalBooks;
            document.getElementById("issuedBooks").innerText = data.issuedBooks;
            document.getElementById("totalStudents").innerText = data.totalStudents;

            let el = document.getElementById("pendingRequests");
            if (el) {
                let pending = parseInt(data.pendingRequests) || 0;
                el.innerText = pending;
                if (pending > 0) {
                    el.style.setProperty("color", "#fde68a", "important");
                    el.style.fontWeight = "bold";
                }
            }
        })
        .catch(err => console.log("ERROR:", err));

    // ADD ROW — sirf minus button, no Delete
    document.getElementById("addRow").addEventListener("click", function () {
        var tableBody = document.getElementById("tableBody");
        var row = document.createElement("tr");
        row.innerHTML = `
            <td><input type="number" class="form-control" placeholder="Book ID" /></td>
            <td><input type="text" class="form-control" placeholder="Book name" /></td>
            <td><input type="text" class="form-control" placeholder="Author" /></td>
            <td><input type="number" class="form-control" placeholder="Qty" /></td>
            <td>
                <button class="minus-btn" style="
                    width:34px; height:34px; border-radius:50%;
                    background:#dc2626 !important; border:none;
                    color:white; font-size:22px; font-weight:bold;
                    cursor:pointer; line-height:1;
                    display:flex; align-items:center; justify-content:center; margin:auto;">
                    −
                </button>
            </td>`;
        tableBody.appendChild(row);

        row.querySelector(".minus-btn").addEventListener("click", function () {
            row.remove();
        });
    });

    // SUBMIT
    document.getElementById("submitData").addEventListener("click", function () {
        var rows = document.querySelectorAll("#tableBody tr");
        var data = [];

        rows.forEach(row => {
            var inputs = row.querySelectorAll("input");
            data.push({
                bookId: parseInt(inputs[0].value),
                name: inputs[1].value,
                author: inputs[2].value,
                quantity: parseInt(inputs[3].value)
            });
        });

        fetch('/Home/SaveBooks', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        })
            .then(res => res.json())
            .then(res => alert("Saved Successfully"))
            .catch(err => console.log(err));
    });

});