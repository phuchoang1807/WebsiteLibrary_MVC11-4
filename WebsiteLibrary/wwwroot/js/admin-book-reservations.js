// Giả lập dữ liệu đặt mượn sách
let reservations = [
    {
      email: "ducphuc@gmail.com",
      book: "Khóa học JavaScript",
      quantity: 1,
      status: "sẵn sàng",
      createdDate: "2025-03-12",
    },
  ];
  
  // Khởi tạo trang
  document.addEventListener('DOMContentLoaded', function () {
    renderReservations();
  });
  
  // Hiển thị danh sách đặt mượn sách
  function renderReservations(searchTerm = '') {
    const tbody = document.querySelector('#order-list tbody');
    tbody.innerHTML = '';
  
    const filteredReservations = reservations.filter(reservation =>
      reservation.email.toLowerCase().includes(searchTerm.toLowerCase()) ||
      reservation.book.toLowerCase().includes(searchTerm.toLowerCase())
    );
  
    if (filteredReservations.length === 0) {
      tbody.innerHTML = '<tr><td colspan="6" style="text-align: center;">Trống!</td></tr>';
      return;
    }
  
    filteredReservations.forEach((reservation, index) => {
      const row = document.createElement('tr');
      const createdDate = new Date(reservation.createdDate);
      const formattedDate = createdDate.toLocaleDateString('vi-VN', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric'
      });
  
      row.innerHTML = `
        <td data-label="Email người đặt">${reservation.email}</td>
        <td data-label="Sách">${reservation.book}</td>
        <td data-label="Số lượng">${reservation.quantity}</td>
        <td data-label="Trạng thái">${reservation.status}</td>
        <td data-label="Ngày tạo">${formattedDate}</td>
        <td data-label="Hành động">
          <a href="#" class="btn btn-approve" onclick="showApproveModal(${index})">Duyệt</a>
          <a href="#" class="btn btn-cancel" onclick="showCancelModal(${index})">Hủy</a>
        </td>
      `;
      tbody.appendChild(row);
    });
  }
  
  // Tìm kiếm đặt mượn sách
  function searchReservations() {
    const searchTerm = document.getElementById('search-input').value;
    renderReservations(searchTerm);
    const clearSearchIcon = document.getElementById('clear-search');
    clearSearchIcon.style.display = searchTerm ? 'block' : 'none';
  }
  
  // Xóa nội dung tìm kiếm
  function clearSearch() {
    document.getElementById('search-input').value = '';
    renderReservations();
    document.getElementById('clear-search').style.display = 'none';
  }
  
  // Thêm sự kiện input để tìm kiếm realtime
  document.getElementById('search-input').addEventListener('input', searchReservations);
  
  // Hiển thị modal xác nhận duyệt
  function showApproveModal(index) {
    const reservation = reservations[index];
    const modal = document.createElement('div');
    modal.className = 'modal';
    modal.id = 'approve-modal';
    modal.innerHTML = `
      <div class="modal-content">
        <span class="close-button" onclick="closeModal('approve-modal')">×</span>
        <h2>Xác nhận duyệt</h2>
        <p>Bạn có chắc chắn muốn duyệt yêu cầu đặt mượn sách "<strong>${reservation.book}</strong>" của <strong>${reservation.email}</strong> không?</p>
        <div class="modal-actions">
          <button class="btn btn-save" onclick="approveReservation(${index})">Duyệt</button>
          <button class="btn btn-cancel" onclick="closeModal('approve-modal')">Hủy</button>
        </div>
      </div>
    `;
    document.body.appendChild(modal);
    modal.style.display = 'block';
    modal.classList.add('show');
  }
  
  // Hiển thị modal xác nhận hủy
  function showCancelModal(index) {
    const reservation = reservations[index];
    const modal = document.createElement('div');
    modal.className = 'modal';
    modal.id = 'cancel-modal';
    modal.innerHTML = `
      <div class="modal-content">
        <span class="close-button" onclick="closeModal('cancel-modal')">×</span>
        <h2>Xác nhận hủy</h2>
        <p>Bạn có chắc chắn muốn hủy yêu cầu đặt mượn sách "<strong>${reservation.book}</strong>" của <strong>${reservation.email}</strong> không?</p>
        <div class="modal-actions">
          <button class="btn btn-danger" onclick="cancelReservation(${index})">Hủy</button>
          <button class="btn btn-cancel" onclick="closeModal('cancel-modal')">Không</button>
        </div>
      </div>
    `;
    document.body.appendChild(modal);
    modal.style.display = 'block';
    modal.classList.add('show');
  }
  
  // Đóng modal
  function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    modal.classList.remove('show');
    setTimeout(() => {
      modal.remove();
    }, 300);
  }
  
  // Duyệt yêu cầu đặt mượn
  function approveReservation(index) {
    const reservation = reservations[index];
  
    // Tạo phiếu mượn trả mới
    const borrowRecord = {
      borrowId: "PM" + Math.floor(Math.random() * 1000).toString().padStart(3, "0"),
      readerId: "DG" + Math.floor(Math.random() * 1000).toString().padStart(4, "0"),
      readerName: reservation.email.split('@')[0], // Lấy phần trước email làm tên
      copyId: "BS" + Math.floor(Math.random() * 1000).toString().padStart(3, "0"),
      bookName: reservation.book,
      borrowDate: new Date().toISOString().split('T')[0], // Ngày mượn là ngày hiện tại
      dueDate: new Date(new Date().setDate(new Date().getDate() + 7)).toISOString().split('T')[0], // Hạn trả sau 7 ngày
      status: "Đang mượn"
    };
  
    // Lưu phiếu mượn trả vào localStorage
    let borrowRecords = JSON.parse(localStorage.getItem('borrowRecords')) || [];
    borrowRecords.push(borrowRecord);
    localStorage.setItem('borrowRecords', JSON.stringify(borrowRecords));
  
    // Xóa yêu cầu đặt mượn
    reservations.splice(index, 1);
    renderReservations();
  
    // Đóng modal và hiển thị thông báo thành công
    closeModal('approve-modal');
    showNotificationModal("Duyệt yêu cầu đặt mượn thành công!");
  }
  
  // Hủy yêu cầu đặt mượn
  function cancelReservation(index) {
    reservations.splice(index, 1);
    renderReservations();
  
    // Đóng modal và hiển thị thông báo thành công
    closeModal('cancel-modal');
    showNotificationModal("Hủy yêu cầu đặt mượn thành công!");
  }
  
  // Hiển thị modal thông báo
  function showNotificationModal(message) {
    const modal = document.createElement('div');
    modal.className = 'notification-modal';
    modal.id = 'notification-modal';
    modal.innerHTML = `
      <div class="notification-content">
        <p>${message}</p>
        <button class="btn btn-ok" onclick="closeNotificationModal()">OK</button>
      </div>
    `;
    document.body.appendChild(modal);
    modal.style.display = 'block';
    modal.classList.add('show');
  }
  
  // Đóng modal thông báo
  function closeNotificationModal() {
    const modal = document.getElementById('notification-modal');
    modal.classList.remove('show');
    setTimeout(() => {
      modal.remove();
    }, 300);
  }
  
  // Đóng modal khi click bên ngoài
  window.onclick = function (event) {
    const modals = ['approve-modal', 'cancel-modal', 'notification-modal'];
    modals.forEach(modalId => {
      const modal = document.getElementById(modalId);
      if (modal && event.target === modal) {
        if (modalId === 'notification-modal') {
          closeNotificationModal();
        } else {
          closeModal(modalId);
        }
      }
    });
  };