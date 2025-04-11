// Đóng modal
function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    modal.classList.remove('show');
    setTimeout(() => modal.style.display = 'none', 300);
}

// Hiển thị modal
function showModal(modalId) {
    const modal = document.getElementById(modalId);
    modal.style.display = 'block';
    setTimeout(() => modal.classList.add('show'), 10);
}

// Hiển thị thông báo
function showCustomAlert(message) {
    document.getElementById('custom-alert-message').textContent = message;
    const alertPopup = document.getElementById('custom-alert-popup');
    alertPopup.style.display = 'block';
    setTimeout(() => alertPopup.classList.add('show'), 10);
}

// Đóng thông báo
function closeCustomAlert() {
    const alertPopup = document.getElementById('custom-alert-popup');
    alertPopup.classList.remove('show');
    setTimeout(() => alertPopup.style.display = 'none', 300);
}

// Kiểm tra ngày hợp lệ
function isValidDate(date) {
    const regex = /^\d{2}\/\d{2}\/\d{4}$/;
    if (!regex.test(date)) return false;

    const [day, month, year] = date.split('/').map(Number);
    if (year !== 2025 || month < 1 || month > 12 || day < 1 || day > 31) return false;

    const daysInMonth = new Date(year, month, 0).getDate();
    return day <= daysInMonth;
}

// Tìm kiếm phiếu nhập
function searchBooks() {
    const searchValue = document.getElementById('search-input').value.toLowerCase();
    document.getElementById('clear-search').style.display = searchValue ? 'block' : 'none';
    updateBooksTable(searchValue);
}

// Xóa tìm kiếm
function clearSearch() {
    document.getElementById('search-input').value = '';
    document.getElementById('clear-search').style.display = 'none';
    updateBooksTable();
}

// Hiển thị popup thêm phiếu nhập
function showAddImportReceiptPopup() {
    document.getElementById('add-created-at').value = '';
    document.getElementById('add-supplier').value = '';
    document.querySelectorAll('#add-import-receipt-modal input').forEach(input => input.style.borderColor = '');

    const bookList = document.getElementById('book-list');
    bookList.innerHTML = '';
    addBookEntry();
    updateDeleteButtons();
    showModal('add-import-receipt-modal');
}

// Thêm mục đầu sách
function addBookEntry() {
    const bookList = document.getElementById('book-list');
    const bookEntry = document.createElement('div');
    bookEntry.className = 'book-entry';
    bookEntry.innerHTML = `
        <div class="form-group">
            <label>Mã đầu sách <span class="required">*</span>:</label>
            <input type="text" class="book-id" required>
        </div>
        <div class="form-group">
            <label>Số lượng <span class="required">*</span>:</label>
            <input type="number" class="quantity" min="1" required>
        </div>
        <div class="form-group">
            <label>Giá nhập <span class="required">*</span>:</label>
            <input type="number" class="price" min="0" step="0.01" required>
        </div>
        <button class="btn btn-delete" onclick="removeBookEntry(this)" style="display: none;">
            <i class="fas fa-times"></i>
        </button>
    `;
    bookList.appendChild(bookEntry);
    updateDeleteButtons();
}

// Cập nhật nút xóa
function updateDeleteButtons() {
    const bookEntries = document.querySelectorAll('.book-entry');
    const deleteButtons = document.querySelectorAll('.btn-delete');
    deleteButtons.forEach(btn => btn.style.display = bookEntries.length > 1 ? 'inline-block' : 'none');
}

// Xóa mục đầu sách
function removeBookEntry(button) {
    const bookList = document.getElementById('book-list');
    if (bookList.children.length > 1) {
        button.parentElement.remove();
        updateDeleteButtons();
    } else {
        showCustomAlert('Phải có ít nhất một đầu sách!');
    }
}

// Xác nhận thêm phiếu nhập
function confirmAddImportReceipt() {
    const createdAtInput = document.getElementById('add-created-at');
    const supplierInput = document.getElementById('add-supplier');
    let isValid = true;

    if (!createdAtInput.value.trim()) {
        isValid = false;
        createdAtInput.style.borderColor = 'red';
    } else {
        createdAtInput.style.borderColor = '';
    }

    if (!supplierInput.value.trim()) {
        isValid = false;
        supplierInput.style.borderColor = 'red';
    } else {
        supplierInput.style.borderColor = '';
    }

    if (!isValid) {
        showCustomAlert('Vui lòng điền đầy đủ thông tin bắt buộc!');
        return;
    }

    if (!isValidDate(createdAtInput.value)) {
        showCustomAlert('Ngày nhập không hợp lệ! Định dạng phải là DD/MM/YYYY và năm phải là 2025.');
        createdAtInput.style.borderColor = 'red';
        return;
    }

    const bookEntries = document.querySelectorAll('.book-entry');
    const books = [];
    for (let entry of bookEntries) {
        const bookId = entry.querySelector('.book-id').value.trim();
        const quantity = parseInt(entry.querySelector('.quantity').value);
        const price = parseFloat(entry.querySelector('.price').value);

        if (!bookId || !quantity || !price) {
            isValid = false;
            if (!bookId) entry.querySelector('.book-id').style.borderColor = 'red';
            if (!quantity) entry.querySelector('.quantity').style.borderColor = 'red';
            if (!price) entry.querySelector('.price').style.borderColor = 'red';
            continue;
        }

        if (quantity < 1) {
            isValid = false;
            entry.querySelector('.quantity').style.borderColor = 'red';
            continue;
        }
        if (price < 0) {
            isValid = false;
            entry.querySelector('.price').style.borderColor = 'red';
            continue;
        }

        entry.querySelector('.book-id').style.borderColor = '';
        entry.querySelector('.quantity').style.borderColor = '';
        entry.querySelector('.price').style.borderColor = '';
        books.push({ bookId, quantity, price });
    }

    if (!isValid) {
        showCustomAlert('Vui lòng điền đầy đủ và hợp lệ thông tin các đầu sách!');
        return;
    }

    if (books.length === 0) {
        showCustomAlert('Phải có ít nhất một đầu sách!');
        return;
    }

    showModal('confirm-add-receipt-popup');
}

// Lưu phiếu nhập
async function saveImportReceipt() {
    const bookEntries = document.querySelectorAll('.book-entry');
    const books = Array.from(bookEntries).map(entry => ({
        bookId: entry.querySelector('.book-id').value.trim(),
        quantity: parseInt(entry.querySelector('.quantity').value),
        price: parseFloat(entry.querySelector('.price').value)
    }));

    const newReceipt = {
        createdAt: document.getElementById('add-created-at').value,
        supplier: document.getElementById('add-supplier').value,
        details: books
    };

    console.log('Dữ liệu gửi lên:', JSON.stringify(newReceipt)); // Debug

    try {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (!token) {
            showCustomAlert('Không tìm thấy CSRF token!');
            return;
        }

        const response = await fetch('/ImportReceipt/AddImportReceipt', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            body: JSON.stringify(newReceipt)
        });

        if (!response.ok) {
            const errorText = await response.text();
            throw new Error(`HTTP ${response.status}: ${errorText}`);
        }

        const result = await response.json();
        if (result.success) {
            updateBooksTable();
            closeModal('confirm-add-receipt-popup');
            closeModal('add-import-receipt-modal');
            showCustomAlert(result.message);
        } else {
            showCustomAlert(result.message || 'Có lỗi xảy ra khi thêm phiếu nhập!');
        }
    } catch (error) {
        console.error('Lỗi chi tiết:', error);
        showCustomAlert(`Đã xảy ra lỗi: ${error.message}`);
    }
}

// Hiển thị chi tiết phiếu nhập
function showViewBookCopyDetailsPopup(receiptId) {
    fetch(`/ImportReceipt/GetImportReceipts`)
        .then(response => response.json())
        .then(receipts => {
            const receipt = receipts.find(r => r.receiptId === receiptId);
            if (receipt) {
                document.getElementById('detail-receipt-id').textContent = receipt.receiptId;
                document.getElementById('detail-created-at').textContent = receipt.createdAt;
                document.getElementById('detail-supplier').textContent = receipt.supplier;

                const bookList = document.getElementById('detail-book-list');
                bookList.innerHTML = '';
                receipt.details.forEach(book => {
                    const row = document.createElement('tr');
                    row.innerHTML = `
                        <td>${book.bookId}</td>
                        <td>${book.quantity}</td>
                        <td>${book.price.toLocaleString('vi-VN')} vnd</td>
                    `;
                    bookList.appendChild(row);
                });

                showModal('book-copy-details-modal');
            } else {
                showCustomAlert('Không tìm thấy phiếu nhập!');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showCustomAlert('Không thể tải chi tiết phiếu nhập!');
        });
}

// Cập nhật bảng
async function updateBooksTable(searchValue = '') {
    try {
        const response = await fetch('/ImportReceipt/GetImportReceipts');
        const receipts = await response.json();
        const filteredReceipts = searchValue
            ? receipts.filter(r => r.receiptId.toLowerCase().includes(searchValue))
            : receipts;

        const tbody = document.querySelector('#book-copy-list tbody');
        tbody.innerHTML = '';
        if (filteredReceipts.length === 0) {
            tbody.innerHTML = '<tr class="empty-row"><td colspan="4">Trống</td></tr>';
        } else {
            filteredReceipts.forEach(receipt => {
                const row = document.createElement('tr');
                row.innerHTML = `
                    <td data-label="Mã phiếu nhập">${receipt.receiptId}</td>
                    <td data-label="Ngày nhập">${receipt.createdAt}</td>
                    <td data-label="Nhà cung cấp">${receipt.supplier}</td>
                    <td data-label="Thao tác">
                        <button class="btn btn-view" onclick="showViewBookCopyDetailsPopup('${receipt.receiptId}')">
                            <i class="fas fa-eye"></i> Xem chi tiết
                        </button>
                    </td>
                `;
                tbody.appendChild(row);
            });
        }
    } catch (error) {
        console.error('Error:', error);
        showCustomAlert('Không thể tải danh sách phiếu nhập!');
    }
}

// Đóng modal khi click bên ngoài
window.onclick = function (event) {
    const modals = document.querySelectorAll('.modal');
    modals.forEach(modal => {
        if (event.target === modal) closeModal(modal.id);
    });
    const alertPopup = document.getElementById('custom-alert-popup');
    if (event.target === alertPopup) closeCustomAlert();
};

// Khởi tạo
document.addEventListener('DOMContentLoaded', () => updateBooksTable());