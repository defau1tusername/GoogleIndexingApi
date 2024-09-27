initializeTasks();
initializeQuotaInfo();
let rowCount = 1;

async function initializeTasks() {
    let skip = 0;
    while (true) {
        const response = await fetch(`/api/tasks?skip=${skip}&limit=30`, {
            method: "GET",
            headers: { "Accept": "application/json" }
        });

        if (response.ok) {
            const tasks = await response.json();
            if (tasks.length == 0)
                break;

            const tableBody = document.querySelector("tbody");


            for (const task of tasks) {
                const newRow = await createRow(task);
                tableBody.append(newRow);

                skip++;
            }
        }
        else
            break;
    }
}

async function initializeQuotaInfo() {
    const response = await fetch("/api/quota", {
        method: "GET",
        headers: { "Accept": "application/json" }
    });

    if (response.ok) {
        const quotaInfo = await response.json();
        const quotaInfoRow = document.getElementById("quota-info");
        quotaInfoRow.append(`${quotaInfo.remainingQuota}/${quotaInfo.totalQuota}`);
    }
}

document.getElementById('indexingForm').addEventListener('submit', async function (event) {
    event.preventDefault();

    const urlList = document.getElementById('urlList').value;
    const isPriority = document.getElementById('priorityCheckbox').checked;
    const urlAction = document.getElementById('actionSelect').value;

    const urls = urlList.split('\n').map(url => url.trim()).filter(url => url !== '');

    const response = await fetch("/api/add-task", {
        method: "POST",
        headers: { "Accept": "application/json", "Content-Type": "application/json" },
        body: JSON.stringify({ urls, isPriority, urlAction })
    });

    if (!response.ok) {
        const error = await response.json();
        alert(error.message);
    }

    const tasks = await response.json();
    const tableBody = document.querySelector("tbody");

    for (const task of tasks) {
        const newRow = await createRow(task);
        tableBody.insertBefore(newRow, tableBody.firstChild);
    }

    changeNumbers();

    this.reset();
});

async function createRow(task) {
    const row = document.createElement("tr");
    row.setAttribute("data-task-id", task.id);

    const numberCell = document.createElement("td");
    numberCell.textContent = rowCount++;
    row.appendChild(numberCell);

    const timeCell = document.createElement("td");
    timeCell.textContent = formatTime(task.addTime);
    row.appendChild(timeCell);

    const urlCell = document.createElement("td");
    urlCell.textContent = task.url;
    urlCell.style.textAlign = "left";
    row.appendChild(urlCell);

    const actionCell = document.createElement("td");
    actionCell.textContent = task.urlAction;
    if (task.isPriority) actionCell.textContent += "+";
    row.appendChild(actionCell);

    if (!task.isCompleted) {
        row.appendChild(createEmptyCell());
        row.appendChild(createEmptyCell());
    } else {
        const responseTimeCell = document.createElement("td");
        responseTimeCell.textContent = formatTime(task.googleResponse.time);
        row.appendChild(responseTimeCell);

        const googleResponseCell = document.createElement("td");
        googleResponseCell.textContent = `${task.googleResponse.statusCode} (${task.googleResponse.message})`;
        if (task.googleResponse.errorReason) googleResponseCell.title = task.googleResponse.errorReason;
        row.appendChild(googleResponseCell);
    }

    const actionsCell = document.createElement("td");
    const deleteBtn = document.createElement("button");
    deleteBtn.className = "remove-btn";
    const deleteIcon = document.createElement("img");
    deleteIcon.className = "delete-icon";
    deleteBtn.appendChild(deleteIcon);
    deleteBtn.addEventListener("click", async () => await deleteTask(task.id));
    actionsCell.appendChild(deleteBtn);
    row.appendChild(actionsCell);

    return row;
}

async function deleteTask(id) {
    if (!confirm()) return;

    const response = await fetch(`/api/delete-task/${id}`, {
        method: "DELETE",
        headers: { "Accept": "application/json" }
    });

    document.querySelector(`tr[data-task-id='${id}']`).remove();
}

function formatTime(time) {
    const date = new Date(time);
    const day = String(date.getDate()).padStart(2, '0');
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const year = String(date.getFullYear()).slice(-2);
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${hours}:${minutes} [${day}.${month}.${year}]`;
}

function createEmptyCell() {
    const cell = document.createElement("td");
    cell.textContent = "-";
    return cell;
}

function changeNumbers() {
    number = 1;
    const rows = document.querySelectorAll('table tr');

    rows.forEach(row => {
        const numberCell = row.querySelector('td');

        if (numberCell) {
            numberCell.textContent = number++;
        }
    });
}
