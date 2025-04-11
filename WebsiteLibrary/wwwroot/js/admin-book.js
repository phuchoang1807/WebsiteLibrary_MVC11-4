let books = [];
let bookCopies = [];
let bookImages = {};
let currentBookId = null;

// Đóng modal
function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    modal.classList.remove('show');
    setTimeout(() => {
        modal.style.display = 'none';
    }, 300);
}

// Hiển thị modal
function showModal(modalId) {
    const modal = document.getElementById(modalId);
    modal.style.display = 'block';
    setTimeout(() => {
        modal.classList.add('show');
    }, 10);
}

// Hiển thị thông báo tùy chỉnh
function showCustomAlert(message) {
    const alertPopup = document.getElementById('custom-alert-popup');
    document.getElementById('custom-alert-message').textContent = message;
    alertPopup.style.display = 'block';
    setTimeout(() => {
        alertPopup.classList.add('show');
    }, 10);
}

// Đóng thông báo tùy chỉnh
function closeCustomAlert() {
    const alertPopup = document.getElementById('custom-alert-popup');
    alertPopup.classList.remove('show');
    setTimeout(() => {
        alertPopup.style.display = 'none';
    }, 300);
}

// Chuyển đổi giá trị thể loại thành văn bản
function getCategoryText(categoryValue) {
    const categories = {
        'Tailieuhoctap': 'Tài liệu học tập',
        'Tailieulichsu': 'Tài liệu lịch sử',
        'Sachphattrienbanthan': 'Sách phát triển bản thân',
        'Tieuthuyet': 'Tiểu thuyết'
    };
    return categories[categoryValue] || categoryValue;
}

// Xử lý upload ảnh
function handleImageUpload(event) {
    const input = event.target;
    const preview = document.getElementById('preview-image');
    const previewContainer = document.getElementById('book-image-preview');

    if (input.files && input.files[0]) {
        const reader = new FileReader();
        reader.onload = function (e) {
            preview.src = e.target.result;
            previewContainer.style.display = 'block';
        };
        reader.readAsDataURL(input.files[0]);
    } else {
        preview.src = '#';
        previewContainer.style.display = 'none';
    }
}

// Cập nhật bảng sách
function updateBooksTable(filteredBooks = books) {
    const tbody = document.querySelector('#book-list tbody');
    tbody.innerHTML = '';

    if (filteredBooks.length === 0) {
        const row = document.createElement('tr');
        row.className = 'empty-row';
        row.innerHTML = `<td colspan="10">Trống</td>`;
        tbody.appendChild(row);
    } else {
        filteredBooks.forEach(book => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td data-label="Ảnh bìa">
                    ${book.image ?
                    `<img src="${book.image}" alt="${book.title}" class="book-cover">` :
                    `<div class="no-image">No image</div>`}
                </td>
                <td data-label="Mã đầu sách">${book.id}</td>
                <td data-label="Tiêu đề đầu sách">${book.title}</td>
                <td data-label="Nhà xuất bản">${book.publisher}</td>
                <td data-label="Tác giả">${book.author}</td>
                <td data-label="Số trang">${book.pages}</td>
                <td data-label="Số lượng">${book.quantity}</td>
                <td data-label="Năm xuất bản">${book.year}</td>
                <td data-label="Thể loại">${getCategoryText(book.category)}</td>
                <td data-label="Thao tác">
                    <button class="btn btn-view" onclick="showViewBookCopyDetailsPopup('${book.id}')">
                        <i class="fas fa-eye"></i> Chi tiết
                    </button>
                    <button class="btn btn-delete" onclick="deleteBookConfirm('${book.id}')">
                        <i class="fas fa-trash-alt"></i> Xóa
                    </button>
                </td>
            `;
            tbody.appendChild(row);
        });
    }
}

// Tìm kiếm sách
function searchBooks() {
    const searchInput = document.getElementById('search-input');
    const searchValue = searchInput.value.toLowerCase();
    const clearButton = document.getElementById('clear-search');

    clearButton.style.display = searchValue ? 'block' : 'none';

    const filteredBooks = books.filter(book =>
        book.id.toLowerCase().includes(searchValue) ||
        book.title.toLowerCase().includes(searchValue)
    );
    updateBooksTable(filteredBooks);
}

// Xóa từ khóa tìm kiếm
function clearSearch() {
    const searchInput = document.getElementById('search-input');
    searchInput.value = '';
    document.getElementById('clear-search').style.display = 'none';
    updateBooksTable();
}

// Hiển thị popup thêm sách
function showAddBookPopup() {
    document.getElementById('add-book-id').value = '';
    document.getElementById('add-book-title').value = '';
    document.getElementById('add-book-publisher').value = '';
    document.getElementById('add-book-author').value = '';
    document.getElementById('add-book-pages').value = '';
    document.getElementById('add-book-year').value = '';
    document.getElementById('add-book-category').value = '';
    document.getElementById('add-book-image').value = '';
    document.getElementById('book-image-preview').style.display = 'none';
    showModal('add-book-popup');
}

// Thêm sách
function addBook() {
    const inputs = document.querySelectorAll('#add-book-popup input[required], #add-book-popup select[required]');
    let isValid = true;

    inputs.forEach(input => {
        if (!input.value.trim()) {
            isValid = false;
            input.style.borderColor = 'red';
        } else {
            input.style.borderColor = '';
        }
    });

    if (!isValid) {
        showCustomAlert('Vui lòng điền đầy đủ thông tin bắt buộc!');
        return;
    }

    const bookId = document.getElementById('add-book-id').value.trim();
    if (books.some(book => book.id === bookId)) {
        showCustomAlert('Mã đầu sách đã tồn tại! Vui lòng nhập mã khác.');
        document.getElementById('add-book-id').style.borderColor = 'red';
        return;
    }

    const bookIdPattern = /^[A-Za-z0-9]{1,10}$/;
    if (!bookIdPattern.test(bookId)) {
        showCustomAlert('Mã đầu sách không hợp lệ! Chỉ cho phép chữ cái và số, tối đa 10 ký tự.');
        document.getElementById('add-book-id').style.borderColor = 'red';
        return;
    }

    const pages = parseInt(document.getElementById('add-book-pages').value);
    if (pages < 1) {
        showCustomAlert('Số trang phải lớn hơn 0!');
        document.getElementById('add-book-pages').style.borderColor = 'red';
        return;
    }

    const year = parseInt(document.getElementById('add-book-year').value);
    if (year < 1900 || year > 2025) {
        showCustomAlert('Năm xuất bản phải từ 1900 đến 2025!');
        document.getElementById('add-book-year').style.borderColor = 'red';
        return;
    }

    const imageFile = document.getElementById('add-book-image').files[0];
    const formData = new FormData();
    formData.append('BookId', bookId);
    formData.append('OriginalBookTitle', document.getElementById('add-book-title').value);
    formData.append('Publisher', document.getElementById('add-book-publisher').value);
    formData.append('Author', document.getElementById('add-book-author').value);
    formData.append('PageCount', pages);
    formData.append('PublicationYear', year);
    formData.append('Category', document.getElementById('add-book-category').value);
    if (imageFile) {
        formData.append('imageFile', imageFile);
    }

    fetch('/Books/AddBook', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Lấy lại danh sách sách mới từ server để đảm bảo đồng bộ
                fetch('/Books/GetBooks')
                    .then(response => response.json())
                    .then(updatedBooks => {
                        books = updatedBooks;
                        updateBooksTable();
                        closeModal('add-book-popup');
                        showCustomAlert('Sách đã được thêm thành công!');
                    })
                    .catch(error => {
                        console.error('Error fetching updated books:', error);
                        showCustomAlert('Có lỗi khi làm mới danh sách sách!');
                    });
            } else {
                showCustomAlert(data.message + ': ' + (data.errors ? data.errors.join(', ') : ''));
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showCustomAlert('Có lỗi xảy ra khi thêm sách!');
        });
}

// Xác nhận xóa sách
function deleteBookConfirm(bookId) {
    currentBookId = bookId;
    showModal('delete-book-popup');
}

// Xóa sách
function deleteBook() {
    fetch(`/Books/DeleteBook/${currentBookId}`, {
        method: 'DELETE'
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                books = books.filter(b => b.id !== currentBookId);
                bookCopies = bookCopies.filter(bc => bc.id !== currentBookId);
                if (bookImages[currentBookId]) {
                    URL.revokeObjectURL(bookImages[currentBookId]);
                    delete bookImages[currentBookId];
                }
                updateBooksTable();
                closeModal('delete-book-popup');
                showCustomAlert('Sách đã được xóa!');
            } else {
                showCustomAlert(data.message);
            }
        })
        .catch(error => {
            console.error('Error:', error);
            showCustomAlert('Có lỗi xảy ra khi xóa sách!');
        });
}

// Hiển thị popup chi tiết bản sao sách
function showViewBookCopyDetailsPopup(bookId) {
    fetch(`/Books/GetBookCopies?bookId=${bookId}`)
        .then(response => response.json())
        .then(data => {
            const tbody = document.getElementById('book-copy-details-tbody');
            tbody.innerHTML = '';
            if (!data || data.length === 0) {
                const row = document.createElement('tr');
                row.className = 'empty-row';
                row.innerHTML = '<td colspan="4">Trống</td>';
                tbody.appendChild(row);
            } else {
                data.forEach(copy => {
                    const row = document.createElement('tr');
                    row.innerHTML = `
                    <td data-label="Mã bản sao sách">${copy.copyId}</td>
                    <td data-label="Mã đầu sách">${copy.bookId}</td>
                    <td data-label="Tình trạng">${copy.status}</td>
                    <td data-label="Ngày nhập">${copy.createdAt}</td>
                `;
                    tbody.appendChild(row);
                });
            }
            showModal('book-copy-details-modal');
        })
        .catch(error => console.error('Error:', error));
}

// Đóng modal khi click bên ngoài
window.onclick = function (event) {
    const modals = document.querySelectorAll('.modal');
    modals.forEach(modal => {
        if (event.target === modal) {
            closeModal(modal.id);
        }
    });

    const alertPopup = document.getElementById('custom-alert-popup');
    if (event.target === alertPopup) {
        closeCustomAlert();
    }
};

// Khởi tạo khi tải trang
document.addEventListener('DOMContentLoaded', () => {
    fetch('/Books/GetBooks')
        .then(response => response.json())
        .then(data => {
            books = data;
            updateBooksTable();
        })
        .catch(error => console.error('Error:', error));

    document.getElementById('add-book-image').addEventListener('change', handleImageUpload);
});