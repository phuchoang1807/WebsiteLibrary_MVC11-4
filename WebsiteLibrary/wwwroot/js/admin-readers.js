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

// Kiểm tra định dạng email
function isValidEmail(email) {
    return email.endsWith('@gmail.com') && /^[a-zA-Z0-9._%+-]+@gmail\.com$/.test(email);
}

// Kiểm tra số điện thoại
function isValidPhone(phone) {
    return phone.length === 10 && /^[0-9]{10}$/.test(phone);
}

// Kiểm tra ngày sinh hợp lệ
function isValidDate(dob) {
    const regex = /^\d{2}\/\d{2}\/\d{4}$/;
    if (!regex.test(dob)) return false;

    const [day, month, year] = dob.split('/').map(Number);
    const yearNum = Number(year);
    if (yearNum < 1945 || yearNum > 2015) return false;

    if (day < 1 || day > 31 || month < 1 || month > 12) return false;

    const daysInMonth = new Date(yearNum, month, 0).getDate();
    return day <= daysInMonth;
}

// Lấy danh sách độc giả từ API
async function fetchReaders() {
    const response = await fetch('/Admin/GetReaders');
    const readers = await response.json();
    return readers;
}

// Cập nhật bảng độc giả
function updateReadersTable(readers) {
    const sortedReaders = [...readers].sort((a, b) => b.id.localeCompare(a.id));
    const tbody = document.querySelector('#reader-list tbody');
    tbody.innerHTML = '';
    if (sortedReaders.length === 0) {
        const row = document.createElement('tr');
        row.className = 'empty-row';
        row.innerHTML = `<td colspan="9">Trống</td>`;
        tbody.appendChild(row);
    } else {
        sortedReaders.forEach(reader => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td data-label="Mã độc giả">${reader.id}</td>
                <td data-label="Họ và tên">${reader.name}</td>
                <td data-label="Ngày sinh">${reader.dob}</td>
                <td data-label="Giới tính">${reader.gender}</td>
                <td data-label="Số điện thoại">${reader.phone}</td>
                <td data-label="Email">${reader.email}</td>
                <td data-label="Địa chỉ">${reader.address}</td>
                <td data-label="Trình độ học vấn">${reader.education}</td>
                <td data-label="Thao tác">
                    <button class="btn btn-edit" onclick="showEditReaderPopup('${reader.id}')">
                        <i class="fas fa-edit"></i> Sửa
                    </button>
                    <button class="btn btn-delete" onclick="deleteReaderConfirm('${reader.id}')">
                        <i class="fas fa-trash-alt"></i> Xóa
                    </button>
                </td>
            `;
            tbody.appendChild(row);
        });
    }
}

// Tìm kiếm độc giả
async function searchReaders() {
    const searchInput = document.getElementById('search-input');
    const searchValue = searchInput.value.toLowerCase();
    const clearButton = document.getElementById('clear-search');

    clearButton.style.display = searchValue ? 'block' : 'none';

    const readers = await fetchReaders();
    const filteredReaders = readers.filter(reader =>
        reader.id.toLowerCase().includes(searchValue) ||
        reader.name.toLowerCase().includes(searchValue)
    );
    updateReadersTable(filteredReaders);
}

// Xóa từ khóa tìm kiếm
async function clearSearch() {
    document.getElementById('search-input').value = '';
    document.getElementById('clear-search').style.display = 'none';
    const readers = await fetchReaders();
    updateReadersTable(readers);
}

// Hiển thị popup thêm độc giả
function showAddReaderPopup() {
    document.getElementById('add-reader-name').value = '';
    document.getElementById('add-reader-dob').value = '';
    document.getElementById('add-reader-gender').value = '';
    document.getElementById('add-reader-phone').value = '';
    document.getElementById('add-reader-email').value = '';
    document.getElementById('add-reader-address').value = '';
    document.getElementById('add-reader-education').value = '';
    showModal('add-reader-popup');
}

// Thêm độc giả
async function addReader() {
    const inputs = document.querySelectorAll('#add-reader-popup input[required], #add-reader-popup select[required]');
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

    const dob = document.getElementById('add-reader-dob').value;
    if (!isValidDate(dob)) {
        showCustomAlert('Ngày sinh không hợp lệ! Định dạng phải là DD/MM/YYYY và năm từ 1945 đến 2015.');
        document.getElementById('add-reader-dob').style.borderColor = 'red';
        return;
    }

    const phone = document.getElementById('add-reader-phone').value;
    if (!isValidPhone(phone)) {
        showCustomAlert('Số điện thoại phải có đúng 10 số!');
        document.getElementById('add-reader-phone').style.borderColor = 'red';
        return;
    }

    const email = document.getElementById('add-reader-email').value;
    if (!isValidEmail(email)) {
        showCustomAlert('Email phải có định dạng @gmail.com!');
        document.getElementById('add-reader-email').style.borderColor = 'red';
        return;
    }

    const education = document.getElementById('add-reader-education').value;
    if (education !== "Trung học" && education !== "Cao học") {
        showCustomAlert('Vui lòng chọn trình độ học vấn hợp lệ!');
        document.getElementById('add-reader-education').style.borderColor = 'red';
        return;
    }

    const newReader = {
        name: document.getElementById('add-reader-name').value,
        dateOfBirth: dob.split('/').reverse().join('-'), // Gửi "YYYY-MM-DD"
        gender: document.getElementById('add-reader-gender').value,
        phoneNumber: document.getElementById('add-reader-phone').value,
        email: document.getElementById('add-reader-email').value,
        address: document.getElementById('add-reader-address').value,
        educationLevel: education
    };

    console.log('Dữ liệu gửi lên:', JSON.stringify(newReader));

    const response = await fetch('/Admin/AddReader', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(newReader)
    });
    const result = await response.json();
    console.log('Kết quả từ server:', result);

    if (result.success) {
        const readers = await fetchReaders();
        updateReadersTable(readers);
        closeModal('add-reader-popup');
        showCustomAlert(result.message);
    } else {
        showCustomAlert(result.message || 'Có lỗi xảy ra khi thêm độc giả!');
    }
}

// Hiển thị popup chỉnh sửa độc giả
async function showEditReaderPopup(readerId) {
    const readers = await fetchReaders();
    const reader = readers.find(r => r.id === readerId);
    if (reader) {
        currentReaderId = readerId;
        document.getElementById('edit-reader-name').value = reader.name;
        document.getElementById('edit-reader-dob').value = reader.dob;
        document.getElementById('edit-reader-gender').value = reader.gender;
        document.getElementById('edit-reader-phone').value = reader.phone;
        document.getElementById('edit-reader-email').value = reader.email;
        document.getElementById('edit-reader-address').value = reader.address;
        document.getElementById('edit-reader-education').value = reader.education;
        showModal('edit-reader-popup');
    }
}

// Lưu thông tin đã chỉnh sửa
async function saveEditedReader() {
    const inputs = document.querySelectorAll('#edit-reader-popup input[required], #edit-reader-popup select[required]');
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

    const dob = document.getElementById('edit-reader-dob').value;
    if (!isValidDate(dob)) {
        showCustomAlert('Ngày sinh không hợp lệ! Định dạng phải là DD/MM/YYYY và năm từ 1945 đến 2015.');
        document.getElementById('edit-reader-dob').style.borderColor = 'red';
        return;
    }

    const phone = document.getElementById('edit-reader-phone').value;
    if (!isValidPhone(phone)) {
        showCustomAlert('Số điện thoại phải có đúng 10 số!');
        document.getElementById('edit-reader-phone').style.borderColor = 'red';
        return;
    }

    const email = document.getElementById('edit-reader-email').value;
    if (!isValidEmail(email)) {
        showCustomAlert('Email phải có định dạng @gmail.com!');
        document.getElementById('edit-reader-email').style.borderColor = 'red';
        return;
    }

    const education = document.getElementById('edit-reader-education').value;
    if (education !== "Trung học" && education !== "Cao học") {
        showCustomAlert('Vui lòng chọn trình độ học vấn hợp lệ!');
        document.getElementById('edit-reader-education').style.borderColor = 'red';
        return;
    }

    const updatedReader = {
        name: document.getElementById('edit-reader-name').value,
        dateOfBirth: dob.split('/').reverse().join('-'), // Gửi "YYYY-MM-DD"
        gender: document.getElementById('edit-reader-gender').value,
        phoneNumber: document.getElementById('edit-reader-phone').value,
        email: document.getElementById('edit-reader-email').value,
        address: document.getElementById('edit-reader-address').value,
        educationLevel: education
    };

    const response = await fetch(`/Admin/UpdateReader/${currentReaderId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(updatedReader)
    });
    const result = await response.json();

    if (result.success) {
        const readers = await fetchReaders();
        updateReadersTable(readers);
        closeModal('edit-reader-popup');
        showCustomAlert(result.message);
    } else {
        showCustomAlert('Có lỗi xảy ra khi cập nhật độc giả!');
    }
}

// Xác nhận xóa độc giả
function deleteReaderConfirm(readerId) {
    currentReaderId = readerId;
    showModal('delete-reader-popup');
}

// Xóa độc giả
async function deleteReader() {
    const response = await fetch(`/Admin/DeleteReader/${currentReaderId}`, {
        method: 'DELETE'
    });
    const result = await response.json();

    if (result.success) {
        const readers = await fetchReaders();
        updateReadersTable(readers);
        closeModal('delete-reader-popup');
        showCustomAlert(result.message);
    } else {
        showCustomAlert('Có lỗi xảy ra khi xóa độc giả!');
    }
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

// Khởi tạo bảng khi tải trang
document.addEventListener('DOMContentLoaded', async () => {
    const readers = await fetchReaders();
    updateReadersTable(readers);
});