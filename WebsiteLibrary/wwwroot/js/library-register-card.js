// Lấy ngày hiện tại và hiển thị vào input "Ngày đăng ký" và "Ngày hết hạn"
document.addEventListener('DOMContentLoaded', () => {
    const registerDateInput = document.getElementById('registerDate');
    const expiryDateInput = document.getElementById('expiryDate');

    // Kiểm tra xem các input có tồn tại không
    if (!registerDateInput || !expiryDateInput) {
        console.error('Không tìm thấy input #registerDate hoặc #expiryDate. Kiểm tra lại HTML.');
        return;
    }

    // Lấy ngày hiện tại
    const today = new Date();
    const formattedRegisterDate = `${today.getDate().toString().padStart(2, '0')}/${(today.getMonth() + 1).toString().padStart(2, '0')}/${today.getFullYear()}`;
    registerDateInput.value = formattedRegisterDate;

    // Tính ngày hết hạn (31/12 của năm đăng ký)
    const expiryDate = new Date(today.getFullYear(), 11, 31); // Tháng 11 (0-based) là tháng 12
    const formattedExpiryDate = `${expiryDate.getDate().toString().padStart(2, '0')}/${(expiryDate.getMonth() + 1).toString().padStart(2, '0')}/${expiryDate.getFullYear()}`;
    expiryDateInput.value = formattedExpiryDate;
});

// Chuyển hướng đến trang thanh toán
function proceedToPayment() {
    // Lấy thông tin từ form
    const accountId = document.getElementById('accountId').value;
    const readerName = document.getElementById('readerName').value;
    const registerDate = document.getElementById('registerDate').value;
    const expiryDate = document.getElementById('expiryDate').value;
    const cardPhotoInput = document.getElementById('cardPhoto');
    const cardPhoto = cardPhotoInput.files[0];

    // Kiểm tra xem đã chọn ảnh thẻ chưa
    if (!cardPhoto) {
        alert('Vui lòng chọn ảnh thẻ 3x4!');
        return;
    }

    // Tạo mã thẻ ngẫu nhiên (LIB + 6 chữ số)
    const cardCode = 'LIB' + Math.floor(100000 + Math.random() * 900000);

    // Lưu thông tin vào sessionStorage để sử dụng ở trang thanh toán
    const cardInfo = {
        accountId: accountId,
        readerName: readerName,
        registerDate: registerDate,
        expiryDate: expiryDate,
        cardCode: cardCode,
        cardPhoto: URL.createObjectURL(cardPhoto) // Lưu URL tạm thời
    };
    sessionStorage.setItem('cardInfo', JSON.stringify(cardInfo));

    // Gửi yêu cầu lưu thông tin lên server trước khi chuyển hướng
    const formData = new FormData();
    formData.append('cardPhoto', cardPhoto);

    fetch('/Reader/RegisterCardRequest', {
        method: 'POST',
        body: formData
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                window.location.href = data.redirectUrl; // Điều hướng đến trang thanh toán
            } else {
                alert(data.message || 'Đã xảy ra lỗi khi đăng ký thẻ.');
            }
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Đã xảy ra lỗi khi gửi yêu cầu đăng ký thẻ.');
        });
}