console.log("Issue JS Loaded");
document.getElementById("issueBtn").addEventListener("click", function () {

    var studentName = document.getElementById("studentName").value;
    var bookName = document.getElementById("bookName").value;
    var returnDate = document.getElementById("returnDate").value;
    var email = document.getElementById("studentEmail").value;


    fetch('/Home/IssueBook', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            studentName: studentName,
            bookName: bookName,
            returnDate: returnDate,
            email: email   
        })
    })
        .then(res => res.json())
        .then(data => {
            alert(data.message);
        })
        .catch(err => console.error(err));
});