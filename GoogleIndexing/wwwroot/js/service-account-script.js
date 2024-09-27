initializeServiceAccounts();

let rowCount = 1;
async function initializeServiceAccounts() {
    const response = await fetch("/api/service-accounts", {
        method: "GET",
        headers: { "Accept": "application/json" }
    });

    if (response.ok) {
        const serviceAccounts = await response.json();
        const tableBody = document.querySelector("tbody");

        for (const serviceAccount of serviceAccounts) {
            const newRow = await createRow(serviceAccount);
            tableBody.append(newRow);
        }
    }
}

async function createRow(serviceAccount) {
    const row = document.createElement("tr");

    const numberCell = document.createElement("td");
    numberCell.textContent = rowCount++;
    row.appendChild(numberCell);

    const emailCell = document.createElement("td");
    emailCell.textContent = serviceAccount.shortEmail;
    emailCell.style.textAlign = "left";
    row.appendChild(emailCell);

    const remainingQuotaCell = document.createElement("td");
    remainingQuotaCell.textContent = serviceAccount.remainingQuota;
    row.appendChild(remainingQuotaCell);

    const totalQuotaCell = document.createElement("td");
    totalQuotaCell.textContent = serviceAccount.totalQuota;
    row.appendChild(totalQuotaCell);

    return row;
}