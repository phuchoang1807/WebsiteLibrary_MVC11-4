// Biến toàn cục để lưu trữ dữ liệu
let pendingCards = [];
let approvedCards = [];
let lockedCards = [];
let currentRequestId = null; // Lưu requestId hoặc cardId hiện tại
let isFromPending = false;

// Đóng modal
function closeModal(modalId) {
    const modal = document.getElementById(modalId);
    modal.classList.remove('show');
    setTimeout(() => {
        modal.style.display = 'none';
        if (modalId === 'reader-details-popup') {
            isFromPending = false;
        }
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

// Tải danh sách thẻ đã phê duyệt khi trang được tải
document.addEventListener('DOMContentLoaded', function () {
    fetch('/Admin/GetApprovedCards')
        .then(response => response.json())
        .then(data => {
            approvedCards = data;
            updateApprovedCardsTable();
        })
        .catch(error => {
            console.error('Error loading approved cards:', error);
            showCustomAlert('Lỗi khi tải danh sách thẻ đã phê duyệt.');
        });
});

// Hiển thị popup duyệt thẻ thư viện
function showPendingCardsPopup() {
    fetch('/Admin/GetPendingCards')
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            pendingCards = data;
            const tbody = document.querySelector('#pending-cards-table tbody');
            tbody.innerHTML = '';
            if (pendingCards.length === 0) {
                const row = document.createElement('tr');
                row.className = 'empty-row';
                row.innerHTML = `<td colspan="6">Trống</td>`;
                tbody.appendChild(row);
            } else {
                pendingCards.forEach(card => {
                    const row = document.createElement('tr');
                    row.innerHTML = `
                        <td>${card.cardId}</td>
                        <td>${card.readerId}</td>
                        <td>${card.name}</td>
                        <td>${card.createdDate}</td>
                        <td>${card.duration}</td>
                        <td>
                            <button class="btn btn-details" onclick="showReaderDetails('${card.requestId}', true)">
                                <i class="fas fa-eye"></i> Chi tiết
                            </button>
                            <button class="btn btn-confirm" onclick="confirmCard('${card.requestId}')">
                                <i class="fas fa-check"></i> Xác nhận
                            </button>
                            <button class="btn btn-reject" onclick="rejectCard('${card.requestId}')">
                                <i class="fas fa-times"></i> Loại bỏ
                            </button>
                        </td>
                    `;
                    tbody.appendChild(row);
                });
            }
            showModal('pending-cards-popup');
        })
        .catch(error => {
            console.error('Error loading pending cards:', error);
            showCustomAlert('Lỗi khi tải danh sách thẻ chờ duyệt: ' + error.message);
        });
}

// Xác nhận thẻ
function confirmCard(requestId) {
    currentRequestId = requestId;
    const popup = document.getElementById('confirm-action-popup');
    document.getElementById('confirm-action-title').textContent = 'Xác nhận thẻ';
    document.getElementById('confirm-action-message').textContent = 'Bạn có chắc chắn muốn xác nhận thẻ này?';
    document.getElementById('confirm-action-btn').innerHTML = '<i class="fas fa-check"></i> Xác nhận';
    document.getElementById('confirm-action-btn').onclick = () => approveCard(requestId);
    showModal('confirm-action-popup');
}

// Loại bỏ thẻ
function rejectCard(requestId) {
    currentRequestId = requestId;
    const popup = document.getElementById('confirm-action-popup');
    document.getElementById('confirm-action-title').textContent = 'Loại bỏ thẻ';
    document.getElementById('confirm-action-message').textContent = 'Bạn có chắc chắn muốn loại bỏ thẻ này?';
    document.getElementById('confirm-action-btn').innerHTML = '<i class="fas fa-check"></i> Xác nhận';
    document.getElementById('confirm-action-btn').onclick = () => removeCard(requestId);
    showModal('confirm-action-popup');
}

// Phê duyệt thẻ
function approveCard(requestId) {
    fetch(`/Admin/ApproveCard?requestId=${requestId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                // Cập nhật danh sách
                pendingCards = pendingCards.filter(c => c.requestId !== requestId);
                fetch('/Admin/GetApprovedCards')
                    .then(response => response.json())
                    .then(data => {
                        approvedCards = data;
                        updateApprovedCardsTable();
                        closeModal('confirm-action-popup');
                        showPendingCardsPopup();
                        showCustomAlert('Thẻ đã được xác nhận thành công!');
                    });
            } else {
                showCustomAlert(data.message);
            }
        })
        .catch(error => {
            console.error('Error approving card:', error);
            showCustomAlert('Lỗi khi xác nhận thẻ.');
        });
}

// Loại bỏ thẻ khỏi danh sách chờ
function removeCard(requestId) {
    fetch(`/Admin/RejectCard?requestId=${requestId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                pendingCards = pendingCards.filter(c => c.requestId !== requestId);
                closeModal('confirm-action-popup');
                showPendingCardsPopup();
                showCustomAlert('Thẻ đã bị loại bỏ!');
            } else {
                showCustomAlert(data.message);
            }
        })
        .catch(error => {
            console.error('Error rejecting card:', error);
            showCustomAlert('Lỗi khi loại bỏ thẻ.');
        });
}

// Cập nhật bảng thẻ đã phê duyệt
function updateApprovedCardsTable(filteredCards = approvedCards) {
    const tbody = document.querySelector('#library-cards-list tbody');
    tbody.innerHTML = '';
    if (filteredCards.length === 0) {
        const row = document.createElement('tr');
        row.className = 'empty-row';
        row.innerHTML = `<td colspan="6">Trống</td>`;
        tbody.appendChild(row);
    } else {
        filteredCards.forEach(card => {
            const row = document.createElement('tr');
            row.innerHTML = `
                <td>${card.cardId}</td>
                <td>${card.readerId}</td>
                <td>${card.name}</td>
                <td>${card.createdDate}</td>
                <td>${card.duration}</td>
                <td>
                    <button class="btn btn-details" onclick="showReaderDetails('${card.cardId}', false)">
                        <i class="fas fa-eye"></i> Chi tiết
                    </button>
                </td>
            `;
            tbody.appendChild(row);
        });
    }
}

// Tìm kiếm thẻ
function searchCards() {
    const searchInput = document.getElementById('search-input');
    const searchValue = searchInput.value.toLowerCase();
    const clearButton = document.getElementById('clear-search');

    clearButton.style.display = searchValue ? 'block' : 'none';

    const filteredCards = approvedCards.filter(card =>
        card.cardId.toLowerCase().includes(searchValue) ||
        card.readerId.toLowerCase().includes(searchValue) ||
        card.name.toLowerCase().includes(searchValue)
    );
    updateApprovedCardsTable(filteredCards);
}

// Xóa từ khóa tìm kiếm
function clearSearch() {
    const searchInput = document.getElementById('search-input');
    searchInput.value = '';
    document.getElementById('clear-search').style.display = 'none';
    updateApprovedCardsTable();
}

// Hiển thị chi tiết độc giả/thẻ thư viện
function showReaderDetails(id, fromPending = false) {
    isFromPending = fromPending;
    currentRequestId = id; // Lưu requestId hoặc cardId

    const url = fromPending ? `/Admin/GetPendingCardDetails?requestId=${id}` : `/Admin/GetApprovedCardDetails?cardId=${id}`;
    fetch(url)
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                const card = data.card;
                const title = document.getElementById('reader-details-title');
                const content = document.getElementById('reader-info-content');

                if (fromPending) {
                    title.textContent = 'Thông tin chi tiết độc giả';
                    content.innerHTML = `
                        <p><strong>Mã độc giả:</strong> ${card.readerId}</p>
                        <p><strong>Họ và tên:</strong> ${card.name}</p>
                        <p><strong>Năm sinh:</strong> ${card.birthday}</p>
                        <p><strong>Giới tính:</strong> ${card.gender}</p>
                        <p><strong>Số điện thoại:</strong> ${card.phone}</p>
                        <p><strong>Email:</strong> ${card.email}</p>
                        <p><strong>Trình độ văn hóa:</strong> ${card.education}</p>
                        <p><strong>Địa chỉ:</strong> ${card.address}</p>
                        <p><strong>Ngày tạo thẻ:</strong> ${card.createdDate}</p>
                        <p><strong>Ngày hết hạn:</strong> ${card.duration}</p>
                    `;
                } else {
                    title.textContent = 'Thông tin chi tiết thẻ thư viện';
                    content.innerHTML = `
                        <p><strong>ID thẻ thư viện:</strong> ${card.cardId}</p>
                        <p><strong>ID độc giả:</strong> ${card.readerId}</p>
                        <p><strong>Họ và tên:</strong> ${card.name}</p>
                        <p><strong>Ngày tạo:</strong> ${card.createdDate}</p>
                        <p><strong>Ngày hết hạn:</strong> ${card.duration}</p>
                    `;
                }

                const actionsDiv = document.getElementById('reader-details-actions');
                actionsDiv.style.display = fromPending ? 'none' : 'block';

                showModal('reader-details-popup');
            } else {
                showCustomAlert(data.message);
            }
        })
        .catch(error => {
            console.error('Error loading card details:', error);
            showCustomAlert('Lỗi khi tải thông tin chi tiết.');
        });
}

// Xác nhận khóa thẻ
function lockCardConfirm() {
    showModal('lock-card-popup');
}

// Khóa thẻ
function lockCard() {
    fetch(`/Admin/LockCard?cardId=${currentRequestId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                fetch('/Admin/GetApprovedCards')
                    .then(response => response.json())
                    .then(data => {
                        approvedCards = data;
                        updateApprovedCardsTable();
                        closeModal('lock-card-popup');
                        closeModal('reader-details-popup');
                        showCustomAlert('Thẻ đã bị khóa!');
                    });
            } else {
                showCustomAlert(data.message);
            }
        })
        .catch(error => {
            console.error('Error locking card:', error);
            showCustomAlert('Lỗi khi khóa thẻ.');
        });
}

// Hiển thị danh sách thẻ bị khóa
function showLockedCardsPopup() {
    fetch('/Admin/GetLockedCards')
        .then(response => {
            if (!response.ok) {
                throw new Error(`HTTP error! Status: ${response.status}`);
            }
            return response.json();
        })
        .then(data => {
            lockedCards = data;
            const tbody = document.querySelector('#locked-cards-table tbody');
            tbody.innerHTML = '';
            if (lockedCards.length === 0) {
                const row = document.createElement('tr');
                row.className = 'empty-row';
                row.innerHTML = `<td colspan="6">Trống</td>`;
                tbody.appendChild(row);
            } else {
                lockedCards.forEach(card => {
                    const row = document.createElement('tr');
                    row.innerHTML = `
                        <td>${card.cardId}</td>
                        <td>${card.readerId}</td>
                        <td>${card.name}</td>
                        <td>${card.createdDate}</td>
                        <td>${card.duration}</td>
                        <td>
                            <button class="btn btn-unlock" onclick="unlockCardConfirm('${card.cardId}')">
                                <i class="fas fa-unlock"></i> Mở khóa
                            </button>
                        </td>
                    `;
                    tbody.appendChild(row);
                });
            }
            showModal('locked-cards-popup');
        })
        .catch(error => {
            console.error('Error loading locked cards:', error);
            showCustomAlert('Lỗi khi tải danh sách thẻ bị khóa: ' + error.message);
        });
}

// Xác nhận mở khóa thẻ
function unlockCardConfirm(cardId) {
    currentRequestId = cardId;
    showModal('unlock-card-popup');
}

// Mở khóa thẻ
function unlockCard() {
    fetch(`/Admin/UnlockCard?cardId=${currentRequestId}`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        }
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                fetch('/Admin/GetApprovedCards')
                    .then(response => response.json())
                    .then(data => {
                        approvedCards = data;
                        updateApprovedCardsTable();
                        closeModal('unlock-card-popup');
                        showLockedCardsPopup();
                        showCustomAlert('Thẻ đã được mở khóa!');
                    });
            } else {
                showCustomAlert(data.message);
            }
        })
        .catch(error => {
            console.error('Error unlocking card:', error);
            showCustomAlert('Lỗi khi mở khóa thẻ.');
        });
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