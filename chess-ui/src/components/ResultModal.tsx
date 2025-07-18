// Modal.tsx
import React from "react";

const modalStyle: React.CSSProperties = {
    position: "fixed",
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    background: "rgba(0,0,0,0.3)",
    display: "flex",
    alignItems: "center",
    justifyContent: "center",
    zIndex: 1000,
};
const boxStyle: React.CSSProperties = {
    background: "white",
    borderRadius: "12px",
    padding: "32px",
    boxShadow: "0 8px 32px rgba(0,0,0,0.15)",
    minWidth: "250px",
    textAlign: "center",
};

export default function ResultModal({
    show,
    onClose,
    children,
}: {
    show: boolean;
    onClose: () => void;
    children: React.ReactNode;
}) {
    if (!show) return null;
    return (
        <div style={modalStyle} onClick={onClose}>
            <div style={boxStyle} onClick={(e) => e.stopPropagation()}>
                <div>{children}</div>
                <button style={{ marginTop: 16 }} onClick={onClose}>
                    OK
                </button>
            </div>
        </div>
    );
}
