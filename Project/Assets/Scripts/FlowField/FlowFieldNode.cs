using UnityEngine;

public class FlowFieldNode : BaseNode
{
	public const int k_Obstacle = -1;
	public const int k_NoInit = -2;
	public const int k_NoAngle = -1;

	private int m_distance = k_NoInit; //到目标的移动距离（非欧几里德距离）
	private int m_dir = k_NoAngle;
	private byte m_state; //0=未处理，1=IsOpen, 2=IsClose
	private bool m_isTarget;

	[SerializeField] private MeshRenderer m_bgRenderer;
	private Material m_bgMat;
	[SerializeField] private TextMesh m_text;
	[SerializeField] private Transform m_arrow;

    #region get-set
	public int Distance 
	{ 
		get { return m_distance; } 
		set { m_distance = value; }
	}

	public int Dir 
	{ 
		get { return m_dir; } 
		set { m_dir = value; }
	}

	public byte State 
	{ 
		get { return m_state; } 
		set { m_state = value; }
	}
    #endregion

    public override void Init(int x, int y, byte cost)
	{
		base.Init(x, y, cost);

		gameObject.SetActive(true);

		float widthGap = 1.1f;
		transform.position = new Vector3(x * widthGap, y * widthGap, 0);

		m_bgMat = m_bgRenderer.material;
		m_bgMat.color = GetColor(m_cost);

		m_text.gameObject.SetActive(false);
	}

	public void Reset()
	{
		m_distance = k_NoInit;
		m_dir = k_NoAngle;
		m_state = 0;
		m_bgMat.color = GetColor(m_cost);
	}

	private void ShowValue()
	{
		if (m_distance == k_Obstacle)
		{
			m_text.gameObject.SetActive(false);
			return;
		}

		m_text.gameObject.SetActive(true);
		m_text.text = m_distance.ToString();
	}

	private void HideValue()
	{
		m_text.gameObject.SetActive(false);
	}

	private void ShowArrow()
	{
		if (m_dir < 0)
		{
			HideArrow();
			return;
		}

		m_arrow.gameObject.SetActive(true);
		m_arrow.localRotation = Quaternion.Euler(0, 0, m_dir);
	}

	private void HideArrow()
	{
		m_arrow.gameObject.SetActive(false);
	}

	public void Show(FlowFieldShowType showType)
	{
		switch (showType)
		{
			case FlowFieldShowType.HeatMap:
				ShowValue();
				HideArrow();
				break;
			case FlowFieldShowType.VectorField:
				ShowArrow();
				HideValue();
				break;
			case FlowFieldShowType.All:
				ShowValue();
				ShowArrow();
				break;
			default:
				HideValue();
				HideArrow();
				break;
		}
	}

	private Color GetColor(byte cost)
	{
		if (m_isTarget)
			return Color.red;
		else
			return Define.Cost2Color(cost);
	}

	public void SetIsTarget(bool isTarget)
	{
		m_isTarget = isTarget;
		m_bgMat.color = GetColor(m_cost);
	}
}