// Hiển thị modal
function showModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.style.display = 'block';
        setTimeout(() => modal.classList.add('show'), 10);
    }
}

// Đóng modal
function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('show');
        setTimeout(() => modal.style.display = 'none', 300);
    }
}

// Hiển thị thông báo tùy chỉnh
function showCustomAlert(message) {
    const alertPopup = document.getElementById('custom-alert-popup');
    if (alertPopup) {
        document.getElementById('custom-alert-message').textContent = message;
        alertPopup.style.display = 'block';
        setTimeout(() => alertPopup.classList.add('show'), 10);
    }
}

// Đóng thông báo tùy chỉnh
function closeCustomAlert() {
    const alertPopup = document.getElementById('custom-alert-popup');
    if (alertPopup) {
        alertPopup.classList.remove('show');
        setTimeout(() => alertPopup.style.display = 'none', 300);
    }
}

// Kiểm tra định dạng ngày sinh
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

// Kiểm tra số điện thoại
function isValidPhone(phone) {
    return phone.length === 10 && /^[0-9]{10}$/.test(phone);
}

// Hiển thị popup chỉnh sửa thông tin độc giả
function showEditReaderPopup() {
    const dob = document.querySelector('.library-info-item:nth-child(3) .library-info-value').textContent;
    const gender = document.querySelector('.library-info-item:nth-child(4) .library-info-value').textContent;
    const phone = document.querySelector('.library-info-item:nth-child(5) .library-info-value').textContent;
    const education = document.querySelector('.library-info-item:nth-child(7) .library-info-value').textContent;
    const address = document.querySelector('.library-info-item:nth-child(8) .library-info-value').textContent;

    document.getElementById('edit-reader-dob').value = dob !== "Không có dữ liệu" ? dob : "";
    document.getElementById('edit-reader-gender').value = gender !== "Không có dữ liệu" ? gender : "";
    document.getElementById('edit-reader-phone').value = phone !== "Không có dữ liệu" ? phone : "";
    document.getElementById('edit-reader-education').value = education !== "Không có dữ liệu" ? education : "";
    document.getElementById('edit-reader-address').value = address !== "Không có dữ liệu" ? address : "";

    showModal('edit-reader-popup');
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

    const updatedReader = {
        dateOfBirth: dob.split('/').reverse().join('-'),
        gender: document.getElementById('edit-reader-gender').value,
        phoneNumber: phone,
        educationLevel: document.getElementById('edit-reader-education').value,
        address: document.getElementById('edit-reader-address').value
    };

    try {
        const response = await fetch('/Reader/EditInfo', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(updatedReader)
        });
        const result = await response.json();

        if (result.success) {
            document.querySelector('.library-info-item:nth-child(3) .library-info-value').textContent = dob;
            document.querySelector('.library-info-item:nth-child(4) .library-info-value').textContent = updatedReader.gender;
            document.querySelector('.library-info-item:nth-child(5) .library-info-value').textContent = updatedReader.phoneNumber;
            document.querySelector('.library-info-item:nth-child(7) .library-info-value').textContent = updatedReader.educationLevel;
            document.querySelector('.library-info-item:nth-child(8) .library-info-value').textContent = updatedReader.address;

            closeModal('edit-reader-popup');
            showCustomAlert(result.message);
        } else {
            showCustomAlert(result.message || 'Có lỗi xảy ra khi cập nhật thông tin!');
        }
    } catch (error) {
        showCustomAlert('Lỗi kết nối đến server! Vui lòng thử lại.');
    }
}

// Hiển thị popup đổi mật khẩu
function showChangePasswordPopup() {
    document.getElementById('old-password').value = '';
    document.getElementById('new-password').value = '';
    document.getElementById('confirm-password').value = '';
    showModal('change-password-popup');
}

// Đổi mật khẩu
async function changePassword() {
    const oldPassword = document.getElementById('old-password').value;
    const newPassword = document.getElementById('new-password').value;
    const confirmPassword = document.getElementById('confirm-password').value;

    if (!oldPassword || !newPassword || !confirmPassword) {
        showCustomAlert('Vui lòng điền đầy đủ các trường!');
        return;
    }

    if (newPassword !== confirmPassword) {
        showCustomAlert('Mật khẩu mới và xác nhận mật khẩu không khớp!');
        document.getElementById('new-password').style.borderColor = 'red';
        document.getElementById('confirm-password').style.borderColor = 'red';
        return;
    }

    if (newPassword.length < 6) {
        showCustomAlert('Mật khẩu mới phải có ít nhất 6 ký tự!');
        document.getElementById('new-password').style.borderColor = 'red';
        return;
    }

    const passwordData = {
        oldPassword: oldPassword,
        newPassword: newPassword
    };

    try {
        const response = await fetch('/Reader/ChangePassword', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(passwordData)
        });
        const result = await response.json();

        if (result.success) {
            closeModal('change-password-popup');
            showCustomAlert(result.message);
        } else {
            showCustomAlert(result.message || 'Có lỗi xảy ra khi đổi mật khẩu!');
        }
    } catch (error) {
        showCustomAlert('Lỗi kết nối đến server! Vui lòng thử lại.');
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