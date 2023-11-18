-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local CopyScene_Test = Scripts[000000]

-- ****************************************************** --
--	Hàm này được gọi khi phụ bản bắt đầu
--		copyScene: CopyScene - Phụ bản hiện tại
-- ****************************************************** --
function CopyScene_Test:OnInit(copyScene)

	-- ************************** --
	
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này gọi liên tục mỗi khoảng chừng nào phụ bản còn tồn tại
--		copyScene: CopyScene - Phụ bản hiện tại
-- ****************************************************** --
function CopyScene_Test:OnTick(copyScene)

	-- ************************** --
	
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này gọi khi phụ bản bị đóng
--		copyScene: CopyScene - Phụ bản hiện tại
-- ****************************************************** --
function CopyScene_Test:OnClose(copyScene)

	-- ************************** --
	
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này gọi khi người chơi vào bản đồ
--		copyScene: CopyScene - Phụ bản hiện tại
--		player: Player - Đối tượng người chơi
-- ****************************************************** --
function CopyScene_Test:OnPlayerEnter(copyScene, player)

	-- ************************** --
	
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này gọi khi người chơi giết đối tượng nào khác trong phụ bản
--		copyScene: CopyScene - Phụ bản hiện tại
--		player: Player - Đối tượng người chơi
--		obj: {Monster, Player} -  Đối tượng bị giết, có thể là quái hoặc người chơi khác
-- ****************************************************** --
function CopyScene_Test:OnKillObject(copyScene, player, obj)

	-- ************************** --
	
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này gọi khi người chơi bị chết bởi đối tượng khác trong phụ bản
--		copyScene: CopyScene - Phụ bản hiện tại
--		player: Player - Đối tượng người chơi
--		obj: {Monster, Player} -  Đối tượng giết, có thể là quái hoặc người chơi khác
-- ****************************************************** --
function CopyScene_Test:OnPlayerDie(copyScene, player, obj)

	-- ************************** --
	
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này gọi khi người chơi hồi sinh tại phụ bản
--		copyScene: CopyScene - Phụ bản hiện tại
--		player: Player - Đối tượng người chơi
-- ****************************************************** --
function CopyScene_Test:OnPlayerRelive(copyScene, player)

	-- ************************** --
	
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này gọi khi người chơi bị mất kết nối
--		copyScene: CopyScene - Phụ bản hiện tại
--		player: Player - Đối tượng người chơi
-- ****************************************************** --
function CopyScene_Test:OnPlayerDisconnected(copyScene, player)

	-- ************************** --
	
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này gọi khi người chơi kết nối lại
--		copyScene: CopyScene - Phụ bản hiện tại
--		player: Player - Đối tượng người chơi
-- ****************************************************** --
function CopyScene_Test:OnPlayerReconnected(copyScene, player)

	-- ************************** --
	
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này gọi khi người chơi rời khỏi phụ bản
--		copyScene: CopyScene - Phụ bản hiện tại
--		player: Player - Đối tượng người chơi
-- ****************************************************** --
function CopyScene_Test:OnPlayerLeave(copyScene, player)

	-- ************************** --
	
	-- ************************** --

end