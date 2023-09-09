
public class SideRoom : Room
{
    public RoomDataSO myDataSo;

    public override void InitRoom()
    {
        base.InitRoom();
        if (myDataSo != null)
        {
            //TODO : 실제 방 초기화...
        }
        else InitDefaultRoom();
    }
    private void InitDefaultRoom()
    {
        // TODO : 만약 자신의 RoomDataSO가 null일 때(ex:"Game" 씬에서 Run) 기본 초기화...
    }
}