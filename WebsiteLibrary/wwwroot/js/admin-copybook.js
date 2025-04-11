// Danh sách sách và bản sao (giả lập dữ liệu)
let bookCopies = [
    { bookId: 'BOOK-2025-001', bookTitle: 'Đắc Nhân Tâm', quantity: 7 }
];

let bookCopyDetails = [
    { copyId: 'BS1', bookId: 'BOOK-2025-001', status: 'Có sẵn', createdAt: '2025-01-01' },
    { copyId: 'BS2', bookId: 'BOOK-2025-001', status: 'Đang mượn', createdAt: '2025-01-02' },
    { copyId: 'BS3', bookId: 'BOOK-2025-001', status: 'Hỏng', createdAt: '2025-01-03' },
    { copyId: 'BS4', bookId: 'BOOK-2025-001', status: 'Có sẵn', createdAt: '2025-01-04' },
    { copyId: 'BS5', bookId: 'BOOK-2025-001', status: 'Đang mượn', createdAt: '2025-01-05' },
    { copyId: 'BS6', bookId: 'BOOK-2025-001', status: 'Có sẵn', createdAt: '2025-01-06' },
    { copyId: 'BS7', bookId: 'BOOK-2025-001', status: 'Hỏng', createdAt: '2025-01-07' }
];

// Initialize page
document.addEventListener('DOMContentLoaded', function() {
    renderBookCopies();
});

// Hàm tính số lượng sách sẵn sàng mượn
function getAvailableQuantity(bookId) {
    return bookCopyDetails.filter(copy => 
        copy.bookId === bookId && copy.status === 'Có sẵn'
    ).length;
}

function renderBookCopies(searchTerm = '') {
    const tbody = document.querySelector('#book-copy-list tbody');
    tbody.innerHTML = '';
    const filteredBooks = bookCopies.filter(book => 
        book.bookId.toLowerCase().includes(searchTerm.toLowerCase()) ||
        book.bookTitle.toLowerCase().includes(searchTerm.toLowerCase())
    );
    filteredBooks.forEach(book => {
        const availableQuantity = getAvailableQuantity(book.bookId);
        const row = document.createElement('tr');
        row.innerHTML = `
            <td>${book.bookId}</td>
            <td>${book.bookTitle}</td>
            <td>${book.quantity}</td>
            <td>${availableQuantity}</td>
            <td>
                <a href="#" class="btn" onclick="addBookCopy('${book.bookId}')">Thêm bản sao</a>
                <a href="#" class="btn" onclick="viewBookCopyDetails('${book.bookId}')">Xem chi tiết</a>
            </td>
        `;
        tbody.appendChild(row);
    });
}

function searchBooks() {
    const searchTerm = document.getElementById('search-input').value;
    renderBookCopies(searchTerm);
}

function toggleForm(formId) {
    document.getElementById(formId).style.display =
        document.getElementById(formId).style.display === 'none' ? 'block' : 'none';
}

function toggleDetails() {
    const detailsContainer = document.getElementById('book-copy-details');
    detailsContainer.style.display =
        detailsContainer.style.display === 'none' ? 'block' : 'none';
}

function addBookCopy(bookId) {
    const book = bookCopies.find(b => b.bookId === bookId);
    if (book) {
        document.getElementById('book-id').value = book.bookId;
        document.getElementById('book-title').value = book.bookTitle;
        document.getElementById('book-copy-quantity').value = 1;
        toggleForm('add-book-copy-form');
    }
}

function saveBookCopy() {
    const form = document.getElementById('add-book-copy-form');
    const quantityInput = document.getElementById('book-copy-quantity');
    const bookId = document.getElementById('book-id').value;
    const quantity = parseInt(quantityInput.value);

    if (!quantity || quantity < 1) {
        alert('Vui lòng nhập số lượng bản sao hợp lệ (ít nhất 1)');
        quantityInput.style.borderColor = 'red';
        return;
    }

    const book = bookCopies.find(b => b.bookId === bookId);
    if (book) {
        book.quantity += quantity;
        for (let i = 0; i < quantity; i++) {
            const newCopyId = `BS${bookCopyDetails.length + 1}`;
            const today = new Date();
            const createdAt = today.toISOString().split('T')[0];
            bookCopyDetails.push({
                copyId: newCopyId,
                bookId: bookId,
                status: 'Có sẵn',
                createdAt: createdAt
            });
        }
        renderBookCopies();
        alert('Đã thêm bản sao sách thành công!');
        form.reset();
        toggleForm('add-book-copy-form');
    }
}

function viewBookCopyDetails(bookId) {
    const book = bookCopies.find(b => b.bookId === bookId);
    if (book) {
        document.getElementById('detail-book-id').textContent = book.bookId;
        document.getElementById('detail-book-title').textContent = book.bookTitle;
        document.getElementById('detail-book-quantity').textContent = book.quantity;

        const detailsTbody = document.getElementById('book-copy-details-tbody');
        detailsTbody.innerHTML = '';
        const copies = bookCopyDetails.filter(copy => copy.bookId === bookId);
        copies.forEach(copy => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${copy.copyId}</td>
                <td>${copy.bookId}</td>
                <td>${copy.status}</td>
                <td>${copy.createdAt}</td>
                <td>
                    <a href="#" class="btn" onclick="editBookCopy('${copy.copyId}')">Sửa</a>
                    <a href="#" class="btn" onclick="deleteBookCopy('${copy.copyId}', '${bookId}')">Xóa</a>
                </td>
            `;
            detailsTbody.appendChild(row);
        });

        toggleDetails();
    }
}

function editBookCopy(copyId) {
    const copy = bookCopyDetails.find(c => c.copyId === copyId);
    if (copy) {
        document.getElementById('edit-copy-id').value = copy.copyId;
        document.getElementById('edit-book-id').value = copy.bookId;
        document.getElementById('edit-book-copy-status').value = copy.status;
        document.getElementById('edit-book-copy-created-at').value = copy.createdAt;
        toggleForm('edit-book-copy-form');
    }
}

function updateBookCopy() {
    const form = document.getElementById('edit-book-copy-form');
    const copyId = document.getElementById('edit-copy-id').value;
    const status = document.getElementById('edit-book-copy-status').value;
    const createdAt = document.getElementById('edit-book-copy-created-at').value;

    if (!status || !createdAt) {
        alert('Vui lòng điền đầy đủ thông tin bắt buộc');
        return;
    }

    const copy = bookCopyDetails.find(c => c.copyId === copyId);
    if (copy) {
        copy.status = status;
        copy.createdAt = createdAt;
        const bookId = copy.bookId;
        viewBookCopyDetails(bookId);
        renderBookCopies(); // Cập nhật lại bảng chính
        alert('Đã cập nhật bản sao sách thành công!');
        toggleForm('edit-book-copy-form');
    }
}

function deleteBookCopy(copyId, bookId) {
    if (confirm('Bạn có chắc chắn muốn xóa bản sao sách này?')) {
        bookCopyDetails = bookCopyDetails.filter(c => c.copyId !== copyId);
        const book = bookCopies.find(b => b.bookId === bookId);
        if (book) {
            book.quantity -= 1;
            renderBookCopies();
            viewBookCopyDetails(bookId);
            alert('Đã xóa bản sao sách thành công!');
        }
    }
}