-- Mỗi khi Script được thực thi, ID tương ứng sẽ được lưu trong hệ thống, tại bảng 'Scripts'
-- Dạng đối tượng là dạng Class, được khởi tạo mặc định bởi hệ thống, và sau đó được lưu tại bảng
-- Khi sử dụng dạng Class, cần phải kế thừa Class được hệ thống sinh ra, và dòng lệnh bên dưới để làm điều đó
-- ID Script được khai báo ở file ScriptIndex.xml, thay thế giá trị '000000' bên dưới thành ID tương ứng
local PlayerEvent = Scripts[999999]

-- ****************************************************** --
--	Hàm này được gọi khi người chơi đăng nhập vào Game
--		scene: Scene - Bản đồ hiện tại
--		player: Player - NPC tương ứng
-- ****************************************************** --
function PlayerEvent:OnLogin(scene, player)

	-- ************************** --
	--System.WriteToConsole("PlayerEvent:OnLogin => " .. player:GetName())
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi người chơi đăng xuất khỏi Game
--		scene: Scene - Bản đồ hiện tại
--		player: Player - NPC tương ứng
-- ****************************************************** --
function PlayerEvent:OnLogout(scene, player)

	-- ************************** --
	--System.WriteToConsole("PlayerEvent:OnLogout => " .. player:GetName())
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi người chơi mất kết nối
--		scene: Scene - Bản đồ hiện tại
--		player: Player - NPC tương ứng
-- ****************************************************** --
function PlayerEvent:OnDisconnected(scene, player)

	-- ************************** --
	--System.WriteToConsole("PlayerEvent:OnDisconnected => " .. player:GetName())
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi người chơi kết nối lại
--		scene: Scene - Bản đồ hiện tại
--		player: Player - NPC tương ứng
-- ****************************************************** --
function PlayerEvent:OnReconnect(scene, player)

	-- ************************** --
	--System.WriteToConsole("PlayerEvent:OnReconnect => " .. player:GetName())
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi người chơi chuyển bản đồ
--		scene: Scene - Bản đồ hiện tại
--		player: Player - NPC tương ứng
--		toScene: Scene - Bản đồ đích đến
-- ****************************************************** --
function PlayerEvent:OnChangeScene(scene, player, toScene)

	-- ************************** --
	--System.WriteToConsole("PlayerEvent:OnChangeScene => " .. player:GetName() .. " From: " .. scene:GetName() .. " - To: " .. toScene:GetName())
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi người chơi đặt chân vào bản đồ
--		scene: Scene - Bản đồ hiện tại
--		player: Player - NPC tương ứng
-- ****************************************************** --
function PlayerEvent:OnEnterScene(scene, player)

	-- ************************** --
	--System.WriteToConsole("PlayerEvent:OnEnterScene => " .. player:GetName())
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi người chơi bị chết
--		scene: Scene - Bản đồ hiện tại
--		player: Player - NPC tương ứng
--		killerObj: {Monster, Player} - đối tượng giết
-- ****************************************************** --
function PlayerEvent:OnDie(scene, player, killerObj)

	-- ************************** --
	--System.WriteToConsole("PlayerEvent:OnDie => " .. player:GetName() .. " - Killer: " .. killerObj:GetName())
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi người chơi giết đối tượng khác
--		scene: Scene - Bản đồ hiện tại
--		player: Player - NPC tương ứng
--		deadObj: {Monster, Player} - đối tượng bị giết
-- ****************************************************** --
function PlayerEvent:OnKillObject(scene, player, deadObj)

	-- ************************** --
	--System.WriteToConsole("PlayerEvent:OnKillObject => " .. player:GetName() .. " - DeadObj: " .. deadObj:GetName())
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi người chơi hồi sinh
--		scene: Scene - Bản đồ hiện tại
--		player: Player - NPC tương ứng
-- ****************************************************** --
function PlayerEvent:OnRelive(scene, player)

	-- ************************** --
	--System.WriteToConsole("PlayerEvent:OnRelive => " .. player:GetName())
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi người chơi đánh trúng đối tượng
--		scene: Scene - Bản đồ hiện tại
--		player: Player - NPC tương ứng
--		obj: {Monster, Player} - Đối tượng tương ứng
--		nDamage: number - Lượng sát thương
-- ****************************************************** --
function PlayerEvent:OnHitObject(scene, player, obj, nDamage)

	-- ************************** --
	--System.WriteToConsole("PlayerEvent:OnHitObject => " .. player:GetName() .. " - Obj: " .. obj:GetName() .. " - Damage: " .. nDamage)
	-- ************************** --

end

-- ****************************************************** --
--	Hàm này được gọi khi người chơi bị đối tượng đánh trúng
--		scene: Scene - Bản đồ hiện tại
--		player: Player - NPC tương ứng
--		obj: {Monster, Player} - Đối tượng tương ứng
--		nDamage: number - Lượng sát thương
-- ****************************************************** --
function PlayerEvent:OnBeHit(scene, player, obj, nDamage)

	-- ************************** --
	--System.WriteToConsole("PlayerEvent:OnBeHit => " .. player:GetName() .. " - Obj: " .. obj:GetName() .. " - Damage: " .. nDamage)
	-- ************************** --

end