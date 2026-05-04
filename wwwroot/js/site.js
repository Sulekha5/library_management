// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// ✅ Global Toast Function — har jagah use kar sakte ho
function showToast(message, type = "success") {
    // Container banao agar nahi hai
    let container = document.getElementById("toastContainer");
    if (!container) {
        container = document.createElement("div");
        container.id = "toastContainer";
        container.style.cssText = `
            position: fixed; top: 80px; right: 20px;
            z-index: 99999; display: flex;
            flex-direction: column; gap: 10px;`;
        document.body.appendChild(container);
    }

    const colors = {
        success: { bg: "#dcfce7", border: "#16a34a", color: "#15803d", icon: "✅" },
        error: { bg: "#fee2e2", border: "#dc2626", color: "#b91c1c", icon: "❌" },
        warning: { bg: "#fef9c3", border: "#d97706", color: "#b45309", icon: "⚠️" },
        info: { bg: "#ede9fe", border: "#5510a1", color: "#5510a1", icon: "ℹ️" }
    };
    const c = colors[type] || colors.info;

    const toast = document.createElement("div");
    toast.style.cssText = `
        background: ${c.bg}; border: 1.5px solid ${c.border};
        color: ${c.color}; padding: 14px 20px; border-radius: 12px;
        font-size: 14px; font-weight: 500; min-width: 280px;
        box-shadow: 0 4px 16px rgba(0,0,0,0.12);
        display: flex; align-items: center; gap: 10px;
        animation: slideIn 0.3s ease;`;
    toast.innerHTML = `
        <span style="font-size:18px;">${c.icon}</span>
        <span style="flex:1;">${message}</span>
        <span onclick="this.parentElement.remove()" 
              style="cursor:pointer; font-size:18px; opacity:0.6;">×</span>`;

    container.appendChild(toast);

    // 3 second baad auto remove
    setTimeout(() => {
        toast.style.animation = "slideOut 0.3s ease";
        setTimeout(() => toast.remove(), 280);
    }, 3000);
}

// ✅ Styled Confirm Dialog
function showConfirm(message, onConfirm) {
    // Purana dialog remove
    const old = document.getElementById("confirmDialog");
    if (old) old.remove();

    const overlay = document.createElement("div");
    overlay.id = "confirmDialog";
    overlay.style.cssText = `
        position: fixed; top:0; left:0; width:100%; height:100%;
        background: rgba(0,0,0,0.5); z-index: 99998;
        display: flex; align-items: center; justify-content: center;`;

    overlay.innerHTML = `
        <div style="background:white; border-radius:16px; padding:30px;
                    width:360px; box-shadow:0 8px 32px rgba(0,0,0,0.2); text-align:center;">
            <div style="font-size:48px; margin-bottom:12px;">🗑️</div>
            <h5 style="color:#1e1e2f; margin-bottom:8px;">Are you sure?</h5>
            <p style="color:#666; font-size:14px; margin-bottom:24px;">${message}</p>
            <div style="display:flex; gap:12px; justify-content:center;">
                <button id="confirmNo"
                    style="padding:10px 28px; border-radius:10px; border:none;
                           background:#e5e7eb; color:#374151; font-weight:500;
                           cursor:pointer; font-size:14px;">
                    Cancel
                </button>
                <button id="confirmYes"
                    style="padding:10px 28px; border-radius:10px; border:none;
                           background:#dc2626; color:white; font-weight:500;
                           cursor:pointer; font-size:14px;">
                    Yes, Delete
                </button>
            </div>
        </div>`;

    document.body.appendChild(overlay);

    document.getElementById("confirmYes").onclick = () => {
        overlay.remove();
        onConfirm();
    };
    document.getElementById("confirmNo").onclick = () => overlay.remove();
}

// ✅ Session Timeout — 30 min baad auto logout
(function () {
    const TIMEOUT = 30 * 60 * 1000; // 30 minutes
    let timer;

    function resetTimer() {
        clearTimeout(timer);
        timer = setTimeout(() => {
            showToast("Session expired. Logging out...", "warning");
            setTimeout(() => window.location.href = "/Auth/Logout", 2000);
        }, TIMEOUT);
    }

    ["mousemove", "keydown", "click", "scroll"].forEach(e =>
        document.addEventListener(e, resetTimer));

    resetTimer();
})();