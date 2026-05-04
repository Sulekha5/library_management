console.log("Records JS Loaded");
fetch('/Home/GetRecords')
    .then(res => res.json())
    .then(data => {

        // Books table
        let bookRows = '';
        data.books.forEach(b => {
            bookRows += `
    <tr>
        <td>${b.name}</td>
        <td>${b.bookId}</td>
        <td>${b.author}</td>
        <td>${b.quantity}</td>
       <td>
            <button class="btn btn-primary editBook"
            data-id="${b.bookId}"
            data-name="${b.name}"
            data-author="${b.author}"
            data-quantity="${b.quantity}">
            Edit
            </button>

            <button class="btn btn-danger deleteBook"
             data-id="${b.bookId}">
             Delete
              </button>
       </td>
    </tr>`;
        });
        document.querySelector("#booksTable tbody").innerHTML = bookRows;

        // Issued table
        let issuedRows = '';
        data.issued.forEach(i => {
            issuedRows += `
<tr>
    <td>${i.student}</td>
    <td>${i.book}</td>
    <td>${i.issueDate}</td>
    <td>${i.returnDate}</td>
    <td>${i.fine}</td>

    <td>
        <button class="btn btn-warning sendReminder"
            data-student="${i.student}"
            data-book="${i.book}"
            data-date="${i.returnDate}">
            Reminder
        </button>
    </td>

    <td>
        ${i.status === "Pending"
                    ? `
                <button class="btn btn-success approveBtn" data-id="${i.id}">
                    Approve
                </button>

                <button class="btn btn-danger denyBtn" data-id="${i.id}">
                    Deny
                </button>
              `
                    : `<span class="badge bg-info">${i.status}</span>`
                }
    </td>
</tr>`;
        });
        document.querySelector("#issuedTable tbody").innerHTML = issuedRows;
    });


document.addEventListener("click", function (e) {

    if (e.target.classList.contains("sendReminder")) {

        let student = e.target.getAttribute("data-student");
        let book = e.target.getAttribute("data-book");
        let date = e.target.getAttribute("data-date");

        console.log("Sending reminder...");

        fetch('/Home/SendReminderEmail', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded'
            },
            body: `student=${student}&book=${book}&returnDate=${date}`
        })
            .then(res => res.json())
            .then(res => {
                if (res.success) {
                    alert("✅ Email sent successfully");
                } else {
                    alert("❌ Failed: " + res.message);
                }
            })
            .catch(err => {
                console.log(err);
                alert("Error sending email");
            });

    }

});

document.addEventListener("click", function (e) {

    if (e.target.classList.contains("deleteBook")) {

        let id = e.target.getAttribute("data-id");

        if (!confirm("Are you sure to delete this book?")) return;

        fetch('/Home/DeleteBook?id=' + id, {
            method: 'DELETE'
        })
            .then(res => res.json())
            .then(res => {
                if (res.success) {
                    alert("Deleted successfully");
                    location.reload();
                } else {
                    alert("Error: " + res.message);
                }
            })
            .catch(err => {
                console.log(err);
                alert("Error deleting book");
            });
    }

});

document.addEventListener("click", function (e) {

    if (e.target.classList.contains("editBook")) {

        let id = e.target.getAttribute("data-id");
        let name = e.target.getAttribute("data-name");
        let author = e.target.getAttribute("data-author");
        let quantity = e.target.getAttribute("data-quantity");

        // 🔥 modal open karo
        document.getElementById("editModal").style.display = "block";

        // 🔥 data fill karo
        document.getElementById("editId").value = id;
        document.getElementById("editName").value = name;
        document.getElementById("editAuthor").value = author;
        document.getElementById("editQuantity").value = quantity;
    }

});
function closeModal() {
    document.getElementById("editModal").style.display = "none";
}
document.getElementById("updateBook").addEventListener("click", function () {

    let id = document.getElementById("editId").value;
    let name = document.getElementById("editName").value;
    let author = document.getElementById("editAuthor").value;
    let quantity = document.getElementById("editQuantity").value;

    fetch('/Home/UpdateBook', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            bookId: id,
            name: name,
            author: author,
            quantity: quantity
        })
    })
        .then(res => res.json())
        .then(res => {
            alert(res.message);
            location.reload(); // 🔥 table refresh
        })
        .catch(err => {
            console.log(err);
            alert("Error updating book");
        });

});

document.addEventListener("click", function (e) {

    // ✅ APPROVE
    if (e.target.classList.contains("approveBtn")) {

        let id = e.target.getAttribute("data-id");

        fetch('/Home/ApproveRequest?id=' + id, {
            method: 'POST'
        })
            .then(res => res.json())
            .then(res => {
                alert(res.message);
                location.reload();
            });
    }

    // ❌ DENY
    if (e.target.classList.contains("denyBtn")) {

        let id = e.target.getAttribute("data-id");

        fetch('/Home/DenyRequest?id=' + id, {
            method: 'POST'
        })
            .then(res => res.json())
            .then(res => {
                alert(res.message);
                location.reload();
            });
    }

});