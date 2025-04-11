// Khởi tạo trang
document.addEventListener('DOMContentLoaded', function () {
    console.log("Page loaded, fetching loan requests...");
    fetchLoanRequests(); // Gọi hàm lấy dữ liệu khi trang tải
});

let reservations = []; // Biến toàn cục để lưu dữ liệu từ server (chờ duyệt)
let processedRequests = []; // Biến toàn cục để lưu dữ liệu từ server (đã xử lý)

// Lấy dữ liệu từ action GetLoanRequests
function fetchLoanRequests() {
    console.log("Fetching loan requests from /Admin/GetLoanRequests...");
    $.ajax({
        url: '/Admin/GetLoanRequests',
        type: 'GET',
        success: function (data) {
            console.log("Data received:", data);
            reservations = data; // Lưu dữ liệu từ server vào biến toàn cục
            renderReservations(); // Hiển thị dữ liệu
        },
        error: function (xhr, status, error) {
            console.log("Error: " + error);
            console.log("Status: " + status);
            console.log("Response: " + xhr.responseText);
            alert('Đã xảy ra lỗi khi lấy dữ liệu. Vui lòng kiểm tra console để biết chi tiết.');
        }
    });
}

// Lấy danh sách phiếu đã xử lý từ action GetProcessedLoanRequests
function fetchProcessedLoanRequests() {
    console.log("Fetching processed loan requests from /Admin/GetProcessedLoanRequests...");
    $.ajax({
        url: '/Admin/GetProcessedLoanRequests',
        type: 'GET',
        success: function (data) {
            console.log("Processed data received:", data);
            processedRequests = data; // Lưu dữ liệu từ server vào biến toàn cục
            renderProcessedRequestsModal(); // Hiển thị popup
        },
        error: function (xhr, status, error) {
            console.log("Error: " + error);
            console.log("Status: " + status);
            console.log("Response: " + xhr.responseText);
            alert('Đã xảy ra lỗi khi lấy dữ liệu phiếu đã xử lý. Vui lòng kiểm tra console để biết chi tiết.');
        }
    });
}

// Hiển thị danh sách phiếu mượn đặt giữ sách (chờ duyệt)
function renderReservations(searchTerm = '') {
    console.log("Rendering reservations with search term:", searchTerm);
    const tbody = document.querySelector('#order-list tbody');
    if (!tbody) {
        console.log("Error: Could not find #order-list tbody");
        return;
    }
    tbody.innerHTML = '';

    const filteredReservations = reservations.filter(reservation =>
        reservation.requestId.toLowerCase().includes(searchTerm.toLowerCase()) ||
        reservation.cardId.toLowerCase().includes(searchTerm.toLowerCase())
    );

    if (filteredReservations.length === 0) {
        console.log("No reservations to display after filtering.");
        tbody.innerHTML = '<tr><td colspan="5" style="text-align: center;">Trống!</td></tr>';
        return;
    }

    filteredReservations.forEach((reservation, index) => {
        console.log("Rendering reservation:", reservation);
        const row = document.createElement('tr');
        row.innerHTML = `
            <td data-label="Mã yêu cầu">${reservation.requestId}</td>
            <td data-label="Mã thẻ thư viện">${reservation.cardId}</td>
            <td data-label="Ngày gửi yêu cầu">${reservation.requestDate}</td>
            <td data-label="Hạn giữ">${reservation.expirationDate}</td>
            <td data-label="Thao tác">
                <a href="#" class="btn btn-approve" onclick="showApproveWithDurationModal('${reservation.requestId}')">Duyệt</a>
                <a href="#" class="btn btn-cancel" onclick="showCancelModal('${reservation.requestId}')">Hủy</a>
                <a href="#" class="btn btn-view" onclick="showDetailsModal(${index})">Xem chi tiết</a>
            </td>
        `;
        tbody.appendChild(row);
    });
}

// Tìm kiếm phiếu mượn đặt giữ sách
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

// Hiển thị modal duyệt với lựa chọn thời hạn mượn
function showApproveWithDurationModal(requestId) {
    const reservation = reservations.find(r => r.requestId === requestId);
    const modal = document.createElement('div');
    modal.className = 'modal';
    modal.id = 'approve-with-duration-modal';
    modal.innerHTML = `
        <div class="modal-content">
            <span class="close-button" onclick="closeModal('approve-with-duration-modal')">×</span>
            <h2>Tạo phiếu mượn từ yêu cầu</h2>
            <p><strong>Mã yêu cầu:</strong> ${reservation.requestId}</p>
            <p><strong>Mã thẻ thư viện:</strong> ${reservation.cardId}</p>
            <p><strong>Trạng thái:</strong> <span class="status-${reservation.status.toLowerCase().replace(' ', '-')}">${reservation.status}</span></p>
            <hr>
            <table class="book-detail-table">
                <thead>
                    <tr>
                        <th>Số thứ tự</th>
                        <th>Mã bản sao sách</th>
                        <th>Tên sách</th>
                    </tr>
                </thead>
                <tbody>
                    ${reservation.details.map((detail, idx) => `
                        <tr>
                            <td>${idx + 1}</td>
                            <td>${detail.bookCopyId}</td>
                            <td>${detail.bookTitle}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
            <div class="form-group" style="margin-top: 20px;">
                <label>Thời hạn mượn:</label>
                <select id="borrow-duration" required>
                    <option value="7 ngày">7 ngày</option>
                    <option value="14 ngày">14 ngày</option>
                    <option value="30 ngày">30 ngày</option>
                </select>
            </div>
            <div class="modal-actions">
                <button class="btn btn-save" onclick="approveReservation('${reservation.requestId}')">Tạo phiếu mượn</button>
                <button class="btn btn-cancel" onclick="closeModal('approve-with-duration-modal')">Hủy</button>
            </div>
        </div>
    `;
    document.body.appendChild(modal);
    setTimeout(() => {
        modal.style.display = 'block';
        modal.classList.add('show');
    }, 10);
}

// Hiển thị modal xác nhận hủy
function showCancelModal(requestId) {
    const reservation = reservations.find(r => r.requestId === requestId);
    const modal = document.createElement('div');
    modal.className = 'modal';
    modal.id = 'cancel-modal';
    modal.innerHTML = `
        <div class="modal-content">
            <span class="close-button" onclick="closeModal('cancel-modal')">×</span>
            <h2>Xác nhận hủy yêu cầu</h2>
            <p>Bạn có chắc chắn muốn hủy yêu cầu đặt giữ sách với mã yêu cầu <strong>${reservation.requestId}</strong> của thẻ thư viện <strong>${reservation.cardId}</strong> không?</p>
            <div class="modal-actions">
                <button class="btn btn-danger" onclick="cancelReservation('${reservation.requestId}')">Xác nhận</button>
                <button class="btn btn-cancel" onclick="closeModal('cancel-modal')">Hủy</button>
            </div>
        </div>
    `;
    document.body.appendChild(modal);
    setTimeout(() => {
        modal.style.display = 'block';
        modal.classList.add('show');
    }, 10);
}

// Hiển thị modal chi tiết yêu cầu
function showDetailsModal(index) {
    const reservation = reservations[index];
    const modal = document.createElement('div');
    modal.className = 'modal';
    modal.id = 'details-modal';
    modal.innerHTML = `
        <div class="modal-content">
            <span class="close-button" onclick="closeModal('details-modal')">×</span>
            <h2>Chi tiết yêu cầu đặt giữ sách</h2>
            <p><strong>Mã yêu cầu:</strong> ${reservation.requestId}</p>
            <p><strong>Mã thẻ thư viện:</strong> ${reservation.cardId}</p>
            <p><strong>Trạng thái:</strong> <span class="status-${reservation.status.toLowerCase().replace(' ', '-')}">${reservation.status}</span></p>
            <hr>
            <table class="book-detail-table">
                <thead>
                    <tr>
                        <th>Số thứ tự</th>
                        <th>Mã bản sao sách</th>
                        <th>Tên sách</th>
                    </tr>
                </thead>
                <tbody id="detail-book-list">
                    ${reservation.details.map((detail, idx) => `
                        <tr>
                            <td>${idx + 1}</td>
                            <td>${detail.bookCopyId}</td>
                            <td>${detail.bookTitle}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        </div>
    `;
    document.body.appendChild(modal);
    setTimeout(() => {
        modal.style.display = 'block';
        modal.classList.add('show');
    }, 10);
}

// Hiển thị modal danh sách phiếu đã xử lý
function showProcessedRequestsModal() {
    fetchProcessedLoanRequests(); // Gọi AJAX để lấy danh sách phiếu đã xử lý
}

// Hiển thị popup danh sách phiếu đã xử lý
function renderProcessedRequestsModal() {
    const modal = document.createElement('div');
    modal.className = 'modal';
    modal.id = 'processed-requests-modal';
    modal.innerHTML = `
        <div class="modal-content">
            <span class="close-button" onclick="closeModal('processed-requests-modal')">×</span>
            <h2>Danh sách phiếu đã xử lý</h2>
            <table class="book-detail-table">
                <thead>
                    <tr>
                        <th>Mã yêu cầu</th>
                        <th>Mã thẻ thư viện</th>
                        <th>Trạng thái</th>
                        <th>Thao tác</th>
                    </tr>
                </thead>
                <tbody id="processed-requests-list">
                    ${processedRequests.length === 0 ? `
                        <tr>
                            <td colspan="4" style="text-align: center;">Trống!</td>
                        </tr>
                    ` : processedRequests.map((request, index) => `
                        <tr>
                            <td>${request.requestId}</td>
                            <td>${request.cardId}</td>
                            <td class="status-${request.status.toLowerCase().replace(' ', '-')}">${request.status}</td>
                            <td>
                                <a href="#" class="btn btn-view" onclick="showProcessedDetailsModal(${index})">Chi tiết</a>
                            </td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        </div>
    `;
    document.body.appendChild(modal);
    setTimeout(() => {
        modal.style.display = 'block';
        modal.classList.add('show');
    }, 10);
}

// Hiển thị modal chi tiết của phiếu đã xử lý
function showProcessedDetailsModal(index) {
    const request = processedRequests[index];
    const modal = document.createElement('div');
    modal.className = 'modal';
    modal.id = 'processed-details-modal';
    modal.innerHTML = `
        <div class="modal-content">
            <span class="close-button" onclick="closeModal('processed-details-modal')">×</span>
            <h2>Chi tiết phiếu đã xử lý</h2>
            <p><strong>Mã yêu cầu:</strong> ${request.requestId}</p>
            <p><strong>Mã thẻ thư viện:</strong> ${request.cardId}</p>
            <p><strong>Trạng thái:</strong> <span class="status-${request.status.toLowerCase().replace(' ', '-')}">${request.status}</span></p>
            <hr>
            <table class="book-detail-table">
                <thead>
                    <tr>
                        <th>Số thứ tự</th>
                        <th>Mã bản sao sách</th>
                        <th>Tên sách</th>
                    </tr>
                </thead>
                <tbody>
                    ${request.details.map((detail, idx) => `
                        <tr>
                            <td>${idx + 1}</td>
                            <td>${detail.bookCopyId}</td>
                            <td>${detail.bookTitle}</td>
                        </tr>
                    `).join('')}
                </tbody>
            </table>
        </div>
    `;
    document.body.appendChild(modal);
    setTimeout(() => {
        modal.style.display = 'block';
        modal.classList.add('show');
    }, 10);
}

// Đóng modal
function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    modal.classList.remove('show');
    setTimeout(() => {
        modal.style.display = 'none';
        modal.remove();
    }, 300);
}

// Duyệt yêu cầu đặt giữ sách
function approveReservation(requestId) {
    const borrowDuration = document.getElementById('borrow-duration').value;
    $.ajax({
        url: '/Admin/ApproveLoanRequest',
        type: 'POST',
        data: { requestId: requestId, borrowDuration: borrowDuration },
        success: function (response) {
            if (response.success) {
                reservations = reservations.filter(r => r.requestId !== requestId);
                renderReservations();
                closeModal('approve-with-duration-modal');
                alert(response.message);
            } else {
                alert(response.message);
            }
        },
        error: function (xhr, status, error) {
            console.log("Error: " + error);
            alert('Đã xảy ra lỗi khi duyệt yêu cầu. Vui lòng thử lại.');
        }
    });
}

// Hủy yêu cầu đặt giữ sách
function cancelReservation(requestId) {
    $.ajax({
        url: '/Admin/CancelLoanRequest',
        type: 'POST',
        data: { requestId: requestId },
        success: function (response) {
            if (response.success) {
                reservations = reservations.filter(r => r.requestId !== requestId);
                renderReservations();
                closeModal('cancel-modal');
                alert(response.message);
            } else {
                alert(response.message);
            }
        },
        error: function (xhr, status, error) {
            console.log("Error: " + error);
            alert('Đã xảy ra lỗi khi hủy yêu cầu. Vui lòng thử lại.');
        }
    });
}