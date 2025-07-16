import { useState } from "react";

const SetFEN = ({fetchPieces}: {fetchPieces: () => void}) => {
    const [fen, setFen] = useState("");

    const sendFEN = async () => {
        const res = await fetch("http://localhost:5045/api/chess/setfen", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ fen }),
        });
        const data = await res.json();
        console.log("Backend response:", data);

		fetchPieces();
    };

    return (
        <div className="p-4">
            <input
                type="text"
                value={fen}
                onChange={(e) => setFen(e.target.value)}
                className="border p-2 w-full mb-2"
                placeholder="Enter FEN string"
            />
            <button
                onClick={sendFEN}
                className="bg-blue-500 text-white px-4 py-2"
            >
                Send FEN to Backend
            </button>
        </div>
    );
}

export default SetFEN;
