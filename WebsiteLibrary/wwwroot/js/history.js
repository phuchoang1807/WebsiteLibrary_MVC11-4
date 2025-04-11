$(document).ready(function () {
    $('.btn-view-details').click(function () {
        var requestId = $(this).data('request-id');

        // Gọi AJAX để lấy chi tiết
        $.ajax({
            url: '/Reader/GetLoanRequestDetails',
            type: 'GET',
            data: { requestId: requestId },
            success: function (response) {
                // Log response dưới dạng JSON để dễ debug
                console.log('Response from GetLoanRequestDetails:', JSON.stringify(response, null, 2));

                // Kiểm tra response có hợp lệ không
                if (!response || typeof response !== 'object' || !('Success' in response)) {
                    console.error('Invalid response structure:', response);
                    alert('Dữ liệu trả về không hợp lệ.');
                    return;
                }

                if (response.Success) {
                    // Kiểm tra Status
                    if (!response.Status) {
                        console.error('Response is missing Status:', response);
                        alert('Trạng thái yêu cầu mượn không hợp lệ.');
                        return;
                    }

                    // Tạo modal động
                    const modal = document.createElement('div');
                    modal.className = 'modal';
                    modal.id = 'loan-request-details-modal';

                    let content = `
                        <div class="modal-content">
                            <span class="close-button" onclick="closeModal('loan-request-details-modal')">×</span>
                    `;

                    // Kiểm tra Books trước khi sử dụng
                    if (!response.Books || !Array.isArray(response.Books) || response.Books.length === 0) {
                        console.warn('No books found in response:', response);
                        content += `<p>Không có sách nào trong yêu cầu mượn.</p>`;
                    } else {
                        if (response.Status === 'Choduyet') {
                            content += `
                                <h2>Danh sách yêu cầu mượn</h2>
                                <table class="book-detail-table">
                                    <thead>
                                        <tr>
                                            <th>STT</th>
                                            <th>Tên sách</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        ${response.Books.map(book => `
                                            <tr>
                                                <td>${book.Stt || 'N/A'}</td>
                                                <td>${book.BookTitle || 'Không xác định'}</td>
                                            </tr>
                                        `).join('')}
                                    </tbody>
                                </table>
                                <div class="modal-actions">
                                    <button class="btn btn-cancel-request" data-request-id="${requestId}" onclick="cancelLoanRequest('${requestId}')">Hủy yêu cầu mượn</button>
                                </div>
                            `;
                        } else if (response.Status === 'Daduyet') {
                            content += `
                                <h2>Danh sách mượn</h2>
                                <table class="book-detail-table">
                                    <thead>
                                        <tr>
                                            <th>STT</th>
                                            <th>Tên sách</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        ${response.Books.map(book => `
                                            <tr>
                                                <td>${book.Stt || 'N/A'}</td>
                                                <td>${book.BookTitle || 'Không xác định'}</td>
                                            </tr>
                                        `).join('')}
                                    </tbody>
                                </table>
                            `;
                            // Kiểm tra BorrowingSlip
                            if (response.BorrowingSlip) {
                                content += `
                                    <p><strong>Ngày mượn:</strong> ${response.BorrowingSlip.BorrowDate || 'N/A'}</p>
                                    <p><strong>Ngày trả:</strong> ${response.BorrowingSlip.ReturnDate || 'N/A'}</p>
                                    <p><strong>Hạn trả:</strong> ${response.BorrowingSlip.DueDate || 'N/A'}</p>
                                    <p><strong>Trạng thái:</strong> ${response.BorrowingSlip.Status || 'N/A'}</p>
                                `;
                            } else {
                                content += `<p><strong>Thông tin phiếu mượn:</strong> Không tìm thấy phiếu mượn tương ứng.</p>`;
                            }
                        } else {
                            // Trạng thái là Đã hủy hoặc Bị từ chối
                            content += `
                                <h2>Danh sách yêu cầu mượn</h2>
                                <table class="book-detail-table">
                                    <thead>
                                        <tr>
                                            <th>STT</th>
                                            <th>Tên sách</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                        ${response.Books.map(book => `
                                            <tr>
                                                <td>${book.Stt || 'N/A'}</td>
                                                <td>${book.BookTitle || 'Không xác định'}</td>
                                            </tr>
                                        `).join('')}
                                    </tbody>
                                </table>
                            `;
                        }
                    }

                    content += `</div>`;
                    modal.innerHTML = content;
                    document.body.appendChild(modal);

                    // Hiển thị modal với animation
                    setTimeout(() => {
                        modal.style.display = 'block';
                        modal.classList.add('show');
                    }, 10);
                } else {
                    console.warn('Request failed:', response);
                    alert(response.Status || 'Đã xảy ra lỗi khi lấy chi tiết.');
                }
            },
            error: function (xhr, status, error) {
                console.log('AJAX error:', xhr, status, error); // Log lỗi AJAX
                alert('Đã xảy ra lỗi khi tải chi tiết: ' + (xhr.responseText || error));
            }
        });
    });
});

// Hàm đóng modal
function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('show');
        setTimeout(() => {
            modal.style.display = 'none';
            modal.remove();
        }, 300);
    }
}

// Hàm hủy yêu cầu mượn
function cancelLoanRequest(requestId) {
    if (confirm('Bạn có chắc chắn muốn hủy yêu cầu mượn này không?')) {
        $.ajax({
            url: '/Reader/CancelLoanRequest',
            type: 'POST',
            data: { requestId: requestId },
            success: function (response) {
                if (response.success) {
                    alert(response.message);
                    closeModal('loan-request-details-modal');
                    // Tải lại trang để cập nhật danh sách
                    location.reload();
                } else {
                    alert(response.message || 'Đã xảy ra lỗi khi hủy yêu cầu.');
                }
            },
            error: function () {
                alert('Đã xảy ra lỗi khi hủy yêu cầu.');
            }
        });
    }
}