// Biến toàn cục để lưu danh sách phiếu mượn
let borrowRecords = [];

// Khởi tạo trang
document.addEventListener('DOMContentLoaded', function () {
    fetchBorrowRecords();
});

// Lấy danh sách phiếu mượn từ API
async function fetchBorrowRecords() {
    try {
        const response = await fetch('/Admin/GetBorrowRecords');
        if (!response.ok) {
            throw new Error('Network response was not ok ' + response.statusText);
        }
        const data = await response.json();
        borrowRecords = data;
        renderBorrowRecords();
    } catch (error) {
        console.error("Error fetching borrow records:", error);
        showNotificationModal("Lỗi khi tải danh sách phiếu mượn: " + error.message);
    }
}

// Hiển thị danh sách phiếu mượn

function renderBorrowRecords(searchTerm = '') {
    const tbody = document.querySelector('#borrow-records-list tbody');
    tbody.innerHTML = '';

    if (!borrowRecords || borrowRecords.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" style="text-align: center;">Không có dữ liệu!</td></tr>';
        return;
    }

    const filteredRecords = borrowRecords.filter(record =>
        record.borrowId.toLowerCase().includes(searchTerm.toLowerCase()) ||
        record.readerName.toLowerCase().includes(searchTerm.toLowerCase())
    );

    if (filteredRecords.length === 0) {
        tbody.innerHTML = '<tr><td colspan="6" style="text-align: center;">Không tìm thấy dữ liệu!</td></tr>';
        return;
    }

    filteredRecords.forEach(record => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td data-label="Mã phiếu mượn">${record.borrowId}</td>
            <td data-label="Mã thẻ thư viện">${record.cardId}</td>
            <td data-label="Tên độc giả">${record.readerName}</td>
            <td data-label="Ngày mượn">${record.borrowDate}</td>
            <td data-label="Ngày đến hạn">${record.dueDate}</td>
            <td data-label="Thao tác">
                <a href="#" class="btn btn-return" onclick="showReturnModal('${record.borrowId}')">Tạo phiếu trả</a>
                <a href="#" class="btn" onclick="showBorrowDetails('${record.borrowId}')">Chi tiết</a>
            </td>
        `;
        tbody.appendChild(row);
    });
}
// Tìm kiếm phiếu mượn
function searchBorrowRecords() {
    const searchTerm = document.getElementById('search-input').value.trim();
    renderBorrowRecords(searchTerm);
}

// Xóa tìm kiếm
function clearSearch() {
    const searchInput = document.getElementById('search-input');
    searchInput.value = '';
    renderBorrowRecords();
    document.querySelector('.clear-search').style.display = 'none';
}

// Hiển thị nút xóa tìm kiếm khi có nội dung
document.getElementById('search-input').addEventListener('input', function () {
    const clearButton = document.querySelector('.clear-search');
    clearButton.style.display = this.value ? 'block' : 'none';
});
// Hiển thị chi tiết phiếu mượn
function showBorrowDetails(borrowId) {
    const record = borrowRecords.find(r => r.borrowId === borrowId);
    if (!record) {
        showNotificationModal("Không tìm thấy phiếu mượn!");
        return;
    }

    // Điền thông tin vào modal
    document.getElementById('modal-borrow-id').textContent = record.borrowId;
    document.getElementById('modal-borrow-card-id').textContent = record.cardId;
    document.getElementById('modal-borrow-date').textContent = record.borrowDate;
    document.getElementById('modal-borrow-duration').textContent = record.duration;

    // Hiển thị danh sách sách mượn
    const bookList = document.getElementById('modal-book-list');
    bookList.innerHTML = '';
    record.books.forEach(book => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td data-label="Mã sách">${book.bookId}</td>
            <td data-label="Tên sách">${book.name}</td>
            <td data-label="Tình trạng">${book.condition}</td>
        `;
        bookList.appendChild(row);
    });

    // Hiển thị modal
    const modal = document.getElementById('borrow-details-modal');
    modal.classList.add('show');
}

// Đóng modal
function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    modal.classList.remove('show');
}

let currentReturnBorrowId = null;


// Hiển thị modal tạo phiếu trả
function showReturnModal(borrowId) {
    currentReturnBorrowId = borrowId;
    const record = borrowRecords.find(r => r.borrowId === borrowId);
    if (!record) {
        showNotificationModal("Không tìm thấy phiếu mượn!");
        return;
    }

    // Đặt ngày trả mặc định là hiện tại (theo múi giờ Việt Nam)
    const now = new Date();
    const vietnamTime = new Date(now.getTime() + (7 * 60 * 60 * 1000));
    const returnDateInput = document.getElementById('return-date');
    returnDateInput.value = vietnamTime.toISOString().split('T')[0];

    // Hiển thị danh sách sách mượn
    const bookList = document.getElementById('return-book-list');
    bookList.innerHTML = '';
    record.books.forEach(book => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td data-label="Mã sách">${book.bookId}</td>
            <td data-label="Tên sách">${book.name}</td>
            <td data-label="Tình trạng">
                <select class="return-condition" data-book-id="${book.bookId}" required>
                    <option value="" disabled selected>Chọn tình trạng</option>
                    <option value="Nguyên_vẹn">Nguyên vẹn</option>
                    <option value="Hư hỏng">Hư hỏng</option>
                    <option value="Mất">Mất</option>
                </select>
            </td>
        `;
        bookList.appendChild(row);
    });

    // Hiển thị modal
    const modal = document.getElementById('return-modal');
    modal.classList.add('show');
}


// Lưu phiếu trả
async function saveReturnRecord() {
    const returnDateInput = document.getElementById('return-date');
    const returnDate = new Date(returnDateInput.value);
    const record = borrowRecords.find(r => r.borrowId === currentReturnBorrowId);
    if (!record) {
        showNotificationModal("Không tìm thấy phiếu mượn!");
        return;
    }

    const borrowDate = new Date(record.borrowDate.split('/').reverse().join('-')); // Chuyển định dạng dd/MM/yyyy thành yyyy-MM-dd
    if (returnDate < borrowDate) {
        showNotificationModal("Ngày trả không được trước ngày mượn!");
        return;
    }

    const returnConditions = document.querySelectorAll('.return-condition');
    const returnDetails = [];
    let allConditionsSelected = true;

    returnConditions.forEach(condition => {
        const bookId = condition.getAttribute('data-book-id');
        const selectedCondition = condition.value;
        if (!selectedCondition) {
            allConditionsSelected = false;
            return;
        }
        returnDetails.push({
            bookCopyId: bookId,
            condition: selectedCondition
        });
    });

    if (!allConditionsSelected) {
        showNotificationModal("Vui lòng chọn tình trạng cho tất cả sách!");
        return;
    }

    const requestBody = {
        borrowingSlipID: currentReturnBorrowId,
        returnDate: returnDate.toISOString(),
        returnDetails: returnDetails
    };

    console.log("Request body:", JSON.stringify(requestBody));

    try {
        const response = await fetch('/Admin/CreateReturnSlip', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestBody)
        });

        const result = await response.json();
        if (!response.ok) {
            throw new Error(result.message || 'Lỗi khi tạo phiếu trả!');
        }

        // Xóa phiếu mượn khỏi danh sách
        borrowRecords = borrowRecords.filter(r => r.borrowId !== currentReturnBorrowId);
        renderBorrowRecords();

        // Đóng modal và thông báo thành công
        closeModal('return-modal');
        showNotificationModal("Tạo phiếu trả thành công!");
    } catch (error) {
        console.error("Error creating return slip:", error);
        showNotificationModal(error.message);
    }
}



// Hiển thị thông báo
function showNotificationModal(message) {
    const modal = document.getElementById('notification-modal');
    const messageElement = document.getElementById('notification-message');
    messageElement.textContent = message;
    modal.classList.add('show');
}

// Đóng thông báo
function closeNotificationModal() {
    const modal = document.getElementById('notification-modal');
    modal.classList.remove('show');
}

// Hiển thị modal thêm phiếu mượn
function showAddModal() {
    // Reset form
    document.getElementById('card-id').value = '';
    const now = new Date();
    // Điều chỉnh múi giờ Việt Nam (UTC+7)
    const vietnamTime = new Date(now.getTime() + (7 * 60 * 60 * 1000));
    // Chỉ lấy ngày (yyyy-MM-dd)
    document.getElementById('borrow-date').value = vietnamTime.toISOString().split('T')[0];
    const borrowDurationSelect = document.getElementById('borrow-duration');
    borrowDurationSelect.innerHTML = `
        <option value="7 ngày">7 ngày</option>
        <option value="14 ngày" selected>14 ngày</option>
        <option value="30 ngày">30 ngày</option>
    `;
    const bookList = document.getElementById('book-list');
    bookList.innerHTML = `
        <div class="book-item">
            <div class="book-input">
                <input type="text" class="book-id" placeholder="Mã đầu sách (VD: KMn)" required>
                <input type="text" class="book-name" placeholder="Tên sách" readonly>
            </div>
            <button class="btn-remove-book" onclick="removeBook(this)"><i class="fas fa-trash"></i></button>
        </div>
    `;

    // Hiển thị modal
    const modal = document.getElementById('add-modal');
    modal.classList.add('show');

    // Thêm sự kiện tìm kiếm sách
    document.querySelectorAll('.book-id').forEach(input => {
        input.addEventListener('input', async function () {
            const bookId = this.value.trim();
            const bookNameInput = this.nextElementSibling;
            if (bookId) {
                try {
                    const response = await fetch(`/Admin/GetBookById?bookId=${encodeURIComponent(bookId)}`);
                    if (!response.ok) {
                        const errorData = await response.json();
                        throw new Error(errorData.message || 'Không tìm thấy sách!');
                    }
                    const book = await response.json();
                    bookNameInput.value = book.originalBookTitle;
                } catch (error) {
                    bookNameInput.value = error.message;
                    console.error("Error fetching book:", error);
                }
            } else {
                bookNameInput.value = '';
            }
        });
    });
}

// Thêm sách mới vào danh sách
function addBook() {
    const bookList = document.getElementById('book-list');
    const newBookItem = document.createElement('div');
    newBookItem.classList.add('book-item');
    newBookItem.innerHTML = `
        <div class="book-input">
            <input type="text" class="book-id" placeholder="Mã đầu sách (VD: KMn)" required>
            <input type="text" class="book-name" placeholder="Tên sách" readonly>
        </div>
        <button class="btn-remove-book" onclick="removeBook(this)"><i class="fas fa-trash"></i></button>
    `;
    bookList.appendChild(newBookItem);

    // Thêm sự kiện tìm kiếm sách cho input mới
    const newBookIdInput = newBookItem.querySelector('.book-id');
    newBookIdInput.addEventListener('input', async function () {
        const bookId = this.value.trim();
        const bookNameInput = this.nextElementSibling;
        if (bookId) {
            try {
                const response = await fetch(`/Admin/GetBookById?bookId=${encodeURIComponent(bookId)}`);
                if (!response.ok) {
                    const errorData = await response.json();
                    throw new Error(errorData.message || 'Không tìm thấy sách!');
                }
                const book = await response.json();
                bookNameInput.value = book.originalBookTitle;
            } catch (error) {
                bookNameInput.value = error.message;
                console.error("Error fetching book:", error);
            }
        } else {
            bookNameInput.value = '';
        }
    });
}

// Xóa sách khỏi danh sách
function removeBook(button) {
    const bookList = document.getElementById('book-list');
    if (bookList.children.length > 1) {
        button.parentElement.remove();
    } else {
        showNotificationModal("Phải có ít nhất 1 sách trong phiếu mượn!");
    }
}

// Lưu phiếu mượn
async function saveBorrowRecord() {
    const cardId = document.getElementById('card-id').value.trim();
    const borrowDate = document.getElementById('borrow-date').value;
    const borrowDuration = document.getElementById('borrow-duration').value;

    if (!cardId || !borrowDate || !borrowDuration) {
        showNotificationModal("Vui lòng điền đầy đủ thông tin!");
        return;
    }

    const bookIds = [];
    const bookInputs = document.querySelectorAll('.book-id');
    for (let input of bookInputs) {
        const bookId = input.value.trim();
        if (!bookId) {
            showNotificationModal("Vui lòng nhập mã sách cho tất cả sách!");
            return;
        }
        const bookNameInput = input.nextElementSibling;
        if (bookNameInput.value === 'Không tìm thấy sách!' || !bookNameInput.value) {
            showNotificationModal(`Mã sách ${bookId} không hợp lệ!`);
            return;
        }
        bookIds.push(bookId);
    }

    const requestBody = {
        cardID: cardId,
        borrowDate: new Date(borrowDate).toISOString(),
        borrowDuration: borrowDuration,
        bookIds: bookIds
    };

    console.log("Request body:", JSON.stringify(requestBody));

    try {
        const response = await fetch('/Admin/CreateBorrowingSlip', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestBody)
        });

        const result = await response.json();
        if (!response.ok) {
            throw new Error(result.message || 'Lỗi khi tạo phiếu mượn!');
        }

        // Thêm phiếu mượn mới vào danh sách
        borrowRecords.push(result.data);
        renderBorrowRecords();

        // Đóng modal và thông báo thành công
        closeModal('add-modal');
        showNotificationModal("Tạo phiếu mượn thành công!");
    } catch (error) {
        console.error("Error creating borrow slip:", error);
        showNotificationModal(error.message);
    }
}

// Biến toàn cục để lưu lịch sử phiếu mượn trả
let historyRecords = [];

// Hiển thị modal lịch sử phiếu mượn trả
async function showHistoryModal() {
    try {
        const response = await fetch('/Admin/GetBorrowReturnHistory');
        if (!response.ok) {
            throw new Error('Network response was not ok ' + response.statusText);
        }
        historyRecords = await response.json();
        renderHistoryRecords();
        const modal = document.getElementById('history-modal');
        modal.classList.add('show');
    } catch (error) {
        console.error("Error fetching history records:", error);
        showNotificationModal("Lỗi khi tải lịch sử phiếu mượn trả: " + error.message);
    }
}

// Hiển thị danh sách lịch sử phiếu mượn trả
function renderHistoryRecords(searchTerm = '') {
    const tbody = document.querySelector('#history-list tbody');
    tbody.innerHTML = '';

    if (!historyRecords || historyRecords.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" style="text-align: center;">Không có dữ liệu!</td></tr>';
        return;
    }

    const filteredRecords = historyRecords.filter(record =>
        record.borrowId.toLowerCase().includes(searchTerm.toLowerCase()) ||
        record.readerName.toLowerCase().includes(searchTerm.toLowerCase())
    );

    if (filteredRecords.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" style="text-align: center;">Không tìm thấy dữ liệu!</td></tr>';
        return;
    }

    filteredRecords.forEach(record => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td data-label="Mã phiếu mượn">${record.borrowId}</td>
            <td data-label="Mã phiếu trả">${record.returnId}</td>
            <td data-label="Mã thẻ thư viện">${record.cardId}</td>
            <td data-label="Tên độc giả">${record.readerName}</td>
            <td data-label="Ngày mượn">${record.borrowDate}</td>
            <td data-label="Ngày trả">${record.returnDate}</td>
            <td data-label="Thao tác">
                <a href="#" class="btn" onclick="showHistoryDetails('${record.borrowId}')">Chi tiết</a>
            </td>
        `;
        tbody.appendChild(row);
    });
}
// Tìm kiếm lịch sử phiếu mượn trả
function searchHistoryRecords() {
    const searchTerm = document.getElementById('history-search-input').value.trim();
    renderHistoryRecords(searchTerm);
}

// Xóa tìm kiếm lịch sử
function clearHistorySearch() {
    const searchInput = document.getElementById('history-search-input');
    searchInput.value = '';
    renderHistoryRecords();
    document.querySelector('#history-clear-search').style.display = 'none';
}

// Hiển thị nút xóa tìm kiếm khi có nội dung
document.getElementById('history-search-input').addEventListener('input', function () {
    const clearButton = document.querySelector('#history-clear-search');
    clearButton.style.display = this.value ? 'block' : 'none';
});
// Hiển thị chi tiết lịch sử phiếu mượn trả
function showHistoryDetails(borrowId) {
    const record = historyRecords.find(r => r.borrowId === borrowId);
    if (!record) {
        showNotificationModal("Không tìm thấy phiếu mượn trả!");
        return;
    }

    // Điền thông tin vào modal
    document.getElementById('history-modal-borrow-id').textContent = record.borrowId;
    document.getElementById('history-modal-return-id').textContent = record.returnId;
    document.getElementById('history-modal-card-id').textContent = record.cardId;
    document.getElementById('history-modal-reader-name').textContent = record.readerName;
    document.getElementById('history-modal-borrow-date').textContent = record.borrowDate;
    document.getElementById('history-modal-duration').textContent = record.duration;
    document.getElementById('history-modal-return-date').textContent = record.returnDate;

    // Hiển thị danh sách sách
    const bookList = document.getElementById('history-modal-book-list');
    bookList.innerHTML = '';
    record.books.forEach(book => {
        const row = document.createElement('tr');
        row.innerHTML = `
            <td data-label="Mã sách">${book.bookId}</td>
            <td data-label="Tên sách">${book.name}</td>
            <td data-label="Tình trạng">${book.condition}</td>
        `;
        bookList.appendChild(row);
    });

    // Hiển thị modal
    const modal = document.getElementById('history-details-modal');
    modal.classList.add('show');
}