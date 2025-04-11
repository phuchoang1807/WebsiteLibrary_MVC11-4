// Xử lý nhấp chuột trên các mục dropdown
document.querySelectorAll('.dropdown-menu a[data-section]').forEach(item => {
    item.addEventListener('click', (e) => {
        e.preventDefault(); // Ngăn chặn hành vi mặc định của liên kết
        const sectionId = item.getAttribute('data-section');

        // Kiểm tra xem trang hiện tại có phải là Info.cshtml không
        if (window.location.pathname === '/Reader/Info') {
            // Nếu đã ở đúng trang, chỉ cần chuyển đổi section
            document.querySelectorAll('.library-nav-item').forEach(item => item.classList.remove('active'));
            document.querySelectorAll('.library-content-section').forEach(section => section.classList.remove('active'));
            const targetSection = document.getElementById(sectionId);
            if (targetSection) {
                targetSection.classList.add('active');
                document.querySelector(`.library-nav-item[data-target="${sectionId}"]`).classList.add('active');
                window.location.hash = sectionId; // Cập nhật hash
            }
        } else {
            // Nếu không ở trang Info, chuyển hướng đến trang với hash
            window.location.href = `/Reader/Info#${sectionId}`;
        }
    });
});

// Chuyển đổi nội dung sidebar
document.querySelectorAll('.library-nav-item').forEach(item => {
    item.addEventListener('click', () => {
        const targetSection = item.getAttribute('data-target');
        document.querySelectorAll('.library-nav-item').forEach(item => item.classList.remove('active'));
        document.querySelectorAll('.library-content-section').forEach(section => section.classList.remove('active'));
        item.classList.add('active');
        document.getElementById(targetSection).classList.add('active');
        window.location.hash = targetSection; // Cập nhật hash
    });
});

// Xử lý hash khi tải trang
window.addEventListener('load', () => {
    const hash = window.location.hash.substring(1); // Loại bỏ '#' khỏi hash
    if (hash) {
        document.querySelectorAll('.library-nav-item').forEach(item => item.classList.remove('active'));
        document.querySelectorAll('.library-content-section').forEach(section => section.classList.remove('active'));
        const targetSection = document.getElementById(hash);
        if (targetSection) {
            targetSection.classList.add('active');
            document.querySelector(`.library-nav-item[data-target="${hash}"]`).classList.add('active');
        }
    } else {
        // Mặc định hiển thị section "Thông Tin Cá Nhân"
        document.getElementById('library-personal-content')?.classList.add('active');
        document.querySelector('[data-target="library-personal-content"]')?.classList.add('active');
    }
});

document.addEventListener('DOMContentLoaded', () => {
    // Đọc dữ liệu từ localStorage
    const cardRequestData = JSON.parse(localStorage.getItem('cardRequestData'));
    if (!cardRequestData) {
        alert('Không tìm thấy thông tin đăng ký thẻ!');
        window.location.href = '/Reader/LibraryRegisterCard';
        return;
    }

    // Lấy số tiền từ localStorage
    const amount = cardRequestData.amount;

    // Cập nhật thông báo số tiền
    const paymentNote = document.getElementById('paymentNote');
    if (paymentNote) {
        paymentNote.textContent = `Vui lòng thanh toán ${amount}.000 VNĐ trong vòng 30 phút`;
    }

    // Giả lập thanh toán thành công sau 10 giây
    setTimeout(() => {
        // Gửi yêu cầu POST đến action ConfirmPayment
        fetch('/Reader/ConfirmPayment', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
            },
            body: JSON.stringify({
                accountId: cardRequestData.accountId,
                readerName: cardRequestData.readerName,
                registerDate: cardRequestData.registerDate,
                expiryDate: cardRequestData.expiryDate,
                cardPhotoUrl: cardRequestData.cardPhotoUrl,
                amount: cardRequestData.amount
            })
        })
            .then(response => response.json())
            .then(data => {
                if (data.success) {
                    // Xóa dữ liệu trong localStorage
                    localStorage.removeItem('cardRequestData');
                    // Chuyển hướng về trang thông tin độc giả
                    window.location.href = '/Reader/Info#library-card-content';
                } else {
                    alert('Có lỗi xảy ra khi xác nhận thanh toán: ' + data.message);
                }
            })
            .catch(error => {
                console.error('Error:', error);
                alert('Có lỗi xảy ra khi xác nhận thanh toán.');
            });
    }, 10000); // 10 giây
});

// Lưu ảnh tĩnh vào máy
function saveQRCode() {
    const qrImage = document.querySelector('.library-qr-code img');
    const link = document.createElement('a');
    link.href = qrImage.src;
    link.download = 'library-card-qrcode.jpg';
    link.click();
}